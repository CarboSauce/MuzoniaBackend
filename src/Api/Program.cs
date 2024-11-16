using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Muzonia.Api.DepInjection;
using Muzonia.Api.Middleware;
using Muzonia.Core.Common;
using Muzonia.Core.Services;
using Muzonia.Core.Services.Api;
using Muzonia.DbEf;
using Muzonia.DbEf.Entities;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (cfg, logCfg) =>
    {
        logCfg.ReadFrom.Configuration(cfg.Configuration);
    }
);

(var apiConfig, _, var aspireConfig) = CreateConfig(builder);

ConfigureExternalServices(aspireConfig, builder);
ConfigureServices(builder.Services, builder.Environment, builder.Configuration);

var app = builder.Build();

await RunServices(app.Services);

//app.UseHttpLogging();

var appEnv = app.Environment;

app.UseSerilogRequestLogging();

app.UseOpenApi(app, app.Configuration, appEnv)
    .UseHttpsRedirection()
    .UseCors()
    .UseAuthorization()
    .AddStaticFiles(apiConfig, appEnv)
    .AddHubs(app, appEnv);
app.MapControllers();

MapIdentityEndpoints();
app.Run();
return;

(ApiConfig, AdminConfig, AspireConfig) CreateConfig(
    WebApplicationBuilder builder
)
{
    var globalConfig = builder.Services.AddConfig(builder.Configuration);

    return globalConfig;
}

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

static void ConfigureExternalServices(
    AspireConfig aspireConfig,
    WebApplicationBuilder builder
)
{
    builder
        .AddDatabase(aspireConfig, builder.Environment)
        .AddRedis(aspireConfig);
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
    builder.Services.AddProblemDetails();

    services.AddScoped<ClaimsPrincipal>(s =>
    {
        var context = s.GetRequiredService<HttpContextAccessor>().HttpContext;
        ArgumentNullException.ThrowIfNull(context);
        return context.User;
    });
    services.AddScoped(
        typeof(CancellationToken),
        sp =>
        {
            var context = sp.GetRequiredService<HttpContextAccessor>();
            ArgumentNullException.ThrowIfNull(context);
            return context.HttpContext?.RequestAborted
                ?? CancellationToken.None;
        }
    );

    services
        .AddCors()
        .AddServices(apiConfig, config)
        .AddAuth(config, apiConfig, env)
        .AddOpenApiServices(config, env)
        .AddFileWriter(apiConfig, env);

    services.AddSignalR();
    services.AddRouting(o => o.LowercaseUrls = true);
    services.AddControllers(o =>
    {
        o.Filters.Add<ApiExceptionFilter>();
    });
}
