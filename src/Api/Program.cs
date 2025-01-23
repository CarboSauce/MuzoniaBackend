using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Muzonia.Api;
using Muzonia.Api.DepInjection;
using Muzonia.Api.Middleware;
using Muzonia.Core.Common;
using Muzonia.Core.Services;
using Muzonia.Core.Services.Api;
using Muzonia.DbEf;
using Muzonia.DbEf.Entities;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfig();

builder.Host.UseSerilog(
    (cfg, logCfg) =>
    {
        logCfg.ReadFrom.Configuration(cfg.Configuration);
    }
);

var (apiConfig, _, aspireConfig, _) = CreateConfig(builder);

ConfigureExternalServices(aspireConfig, builder);
ConfigureServices(builder.Services, builder.Environment, builder.Configuration);

var app = builder.Build();

await RunServices(app.Services, app.Environment);

app.UseHttpLogging();

var appEnv = app.Environment;

if (appEnv.IsDevelopment())
{
    app.UseSerilogRequestLogging();
}

app.UseOpenApi(app, app.Configuration, appEnv)
    .UseHttpsRedirection()
    .UseCors()
    .UseAuthorization()
    .UseHangfire(appEnv)
    .AddStaticFiles(apiConfig, appEnv)
    .AddHubs(app, appEnv);

app.MapEndpoints();

await app.RunAsync();
return;

(ApiConfig, AdminConfig, AspireConfig, EmailConfig) CreateConfig(
    WebApplicationBuilder builder
) => builder.Services.AddConfig(builder.Configuration);

static void ConfigureExternalServices(
    AspireConfig aspireConfig,
    WebApplicationBuilder builder
)
{
    builder
        .AddDatabase(aspireConfig, builder.Environment)
        .AddRedis(aspireConfig);
}

async Task RunServices(IServiceProvider services, IWebHostEnvironment env)
{
    await using var scope = services.CreateAsyncScope();

    await scope.ServiceProvider.ApplyMigrations();

    var seed = new DbSeed(
        scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>(),
        scope.ServiceProvider.GetRequiredService<ApiDbContext>(),
        scope.ServiceProvider.GetRequiredService<IOptions<AdminConfig>>().Value
    );

    await seed.SeedAdminAsync();

    if (env.IsDevelopment())
    {
        await seed.SeedBasicDataAsync();
    }
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
        var context = s.GetRequiredService<IHttpContextAccessor>().HttpContext;
        ArgumentNullException.ThrowIfNull(context);
        return context.User;
    });
    services.AddScoped(
        typeof(CancellationToken),
        sp =>
        {
            var context = sp.GetRequiredService<IHttpContextAccessor>();
            ArgumentNullException.ThrowIfNull(context);
            return context.HttpContext?.RequestAborted
                ?? CancellationToken.None;
        }
    );

    services
        .AddValidatorsFromAssembly(typeof(Program).Assembly)
        .AddCustomCors(config)
        .AddServices(apiConfig, config)
        .AddAuth(config, apiConfig, env)
        .AddOpenApiServices(config, env)
        .AddFileWriter(apiConfig, env)
        .AddHangfireServices(config, apiConfig, env);

    services.AddSignalR();
    services.AddRouting(o => o.LowercaseUrls = true);
}
