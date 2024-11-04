using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Muzonia.Api.Middleware;
using Muzonia.Api.Utils;
using Muzonia.Core;
using Muzonia.Core.Common;
using Muzonia.Core.Services;
using Muzonia.DbEf;
using Muzonia.DbEf.Entities;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

//var isAspire = builder
//    .Configuration.GetSection("Aspire")
//    .GetValue("UseAspire", false);

var useAspireRedis = builder
    .Configuration.GetSection("Aspire")
    .GetValue("UseRedis", false);

var useAspirePostgres = builder
    .Configuration.GetSection("Aspire")
    .GetValue("UsePostgres", false);

var adminConfig = builder
    .Configuration.GetSection("AdminConfig")
    .Get<AdminConfig>();

var apiConfig = builder.Configuration.Get<ApiConfig>();

ArgumentNullException.ThrowIfNull(apiConfig);
ArgumentNullException.ThrowIfNull(adminConfig);

await ConfigureExternalServices(builder);

// Add services to the container.
ConfigureServices(builder.Services, builder.Environment, builder.Configuration);

var app = builder.Build();

await RunServices(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseHttpLogging();
    app.UseDeveloperExceptionPage();
    // app.UseOpenApi(options =>
    // {
    //     options.Path = "/openapi/{documentName}.json";
    // });

    app.UseSwagger(o =>
    {
        o.RouteTemplate = "/openapi/{documentName}.json";
    });
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/openapi/v1.json", "Muzonia API");
    });

    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();
    app.MapGet("/apiui", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

if (apiConfig.UseStaticFiles)
{
    ConfigureStaticFiles();
}

MapIdentityEndpoints();
app.Run();
return;

void MapIdentityEndpoints()
{
    app.MapIdentityApi<AppUser>();
    app.MapPost(
            "/logout",
            async ([FromServices] SignInManager<AppUser> signInManager) =>
            {
                await signInManager.SignOutAsync();
                return Results.Ok();
            }
        )
        .RequireAuthorization();
}

void ConfigureStaticFiles()
{
    var staticContentRoot = builder.Configuration.GetValue<string?>(
        "StaticContentRoot",
        null
    );

    if (staticContentRoot is not null)
        builder.Environment.WebRootPath = staticContentRoot;

    var staticFileOptions = new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(builder.Environment.WebRootPath, "muzonia")
        ),
        ContentTypeProvider = new FileExtensionContentTypeProvider(),
        RequestPath = "/static",
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append(
                "Cache-Control",
                "public, max-age=259200"
            );
        }
    };

    app.UseStaticFiles(staticFileOptions);

    if (builder.Environment.IsDevelopment())
    {
        app.UseDirectoryBrowser(
            new DirectoryBrowserOptions
            {
                FileProvider = staticFileOptions.FileProvider,
                RequestPath = staticFileOptions.RequestPath
            }
        );
    }
}

Task ConfigureExternalServices(WebApplicationBuilder builder)
{
    builder.Services.AddProblemDetails();

    if (useAspireRedis)
    {
        builder.AddRedisClient("redis");
    }
    else
    {
        builder.Services.AddSingleton<IConnectionMultiplexer>(o =>
        {
            var redis = builder.Configuration.GetConnectionString("redis");
            ArgumentNullException.ThrowIfNull(redis);
            return ConnectionMultiplexer.Connect(redis);
        });
    }

    if (useAspirePostgres)
    {
        builder.AddNpgsqlDbContext<ApiDbContext>("apidb");
    }
    else
    {
        builder.Services.AddDbContext<ApiDbContext>(o =>
        {
            var conn = builder.Configuration.GetConnectionString("apidb");
            o.UseNpgsql(conn, o => o.MigrationsAssembly("DbEf.Postgresql"));
        });
    }
    return Task.CompletedTask;
}

async Task RunServices(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();

    await scope.ServiceProvider.ApplyMigrations();

    var seed = new SeedAdmin(
        scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>(),
        scope.ServiceProvider.GetRequiredService<IOptions<AdminConfig>>().Value
    );

    await seed.SeedAsync();
}

void ConfigureServices(
    IServiceCollection services,
    IWebHostEnvironment env,
    ConfigurationManager config
)
{
    services.RegisterServices();
    ApiConfig.Configure(services, config);
    AdminConfig.Configure(services, config);

    services.AddScoped<ClaimsPrincipal>(s =>
    {
        var context = s.GetRequiredService<IHttpContextAccessor>().HttpContext;
        ArgumentNullException.ThrowIfNull(context);
        return context.User;
    });

    if (apiConfig.UseStaticFiles)
    {
        if (env.IsDevelopment())
            services.AddDirectoryBrowser();
        services.AddScoped<IFileWriter, StaticFileWriter>();
    }

    services.AddCors(options =>
    {
        options.AddDefaultPolicy(policyBuilder =>
        {
            var allowLocalhost = builder.Configuration.GetValue(
                "AllowLocalhost",
                false
            );
            if (allowLocalhost)
            {
                policyBuilder.SetIsOriginAllowed(origin =>
                    new Uri(origin).Host == "localhost"
                );
            }
            policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
    });

    services.AddRouting(o => o.LowercaseUrls = true);
    services.AddControllers(o =>
    {
        o.Filters.Add<ApiExceptionFilter>();
    });
    services.AddEndpointsApiExplorer();

    // Auth
    services.AddAuthorization();
    services
        .AddAuthentication(IdentityConstants.ApplicationScheme)
        .AddCookie(IdentityConstants.ApplicationScheme);

    services
        .AddIdentityCore<AppUser>(o =>
        {
            o.User.RequireUniqueEmail = true;
            if (builder.Environment.IsDevelopment())
            {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 0;
            }
            else
            {
                o.Password.RequireDigit = true;
                o.Password.RequireLowercase = true;
                o.Password.RequireUppercase = true;
                o.Password.RequireNonAlphanumeric = true;
                o.Password.RequiredLength = 8;
            }
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<ApiDbContext>()
        .AddDefaultTokenProviders()
        .AddApiEndpoints();

    if (env.IsDevelopment())
    {
        services.AddHttpLogging(o => { });
        services.AddSwaggerGen(o =>
        {
            //o.MapType<Ulid>(() => new OpenApiSchema { Type = "string" });
            o.OperationFilter<HttpResultsOperationFilter>();
        });
        // services.AddOpenApiDocument(o =>
        // {
        //     o.Title = "Muzonia API";
        // });
    }
}
