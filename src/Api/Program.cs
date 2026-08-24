using System.Security.Claims;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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

builder.AddServiceDefaults();
builder.AddConfig();

builder.Host.UseSerilog(
    (cfg, logCfg) =>
    {
        logCfg.ReadFrom.Configuration(cfg.Configuration);
    }
);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var (apiConfig, _, _) = CreateConfig(builder);

ConfigureExternalServices(builder);
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
    .UseForwardedHeaders()
    .UseCors()
    .UseAuthentication()
    .UseAuthorization()
    .UseHangfire(appEnv)
    .AddStaticFiles(apiConfig, appEnv)
    .AddHubs(app, appEnv);

app.MapGraphQL("/api/graphql");
app.MapEndpoints();

await app.RunAsync();
return;

ConfigExt.ConfigTuple CreateConfig(WebApplicationBuilder builder) =>
    builder.Services.AddConfig(builder.Configuration);

static void ConfigureExternalServices(WebApplicationBuilder builder)
{
    builder.AddDatabase(builder.Environment);
    builder.AddRedis();
    builder.AddAzure(builder.Environment);
}

async Task RunServices(IServiceProvider services, IWebHostEnvironment env)
{
    await using var scope = services.CreateAsyncScope();

    await scope.ServiceProvider.ApplyMigrations();
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
        .AddValidatorsFromAssemblyContaining(typeof(Program))
        .AddCustomCors(config)
        .AddServices(apiConfig, config)
        .AddAuth(config, apiConfig, env)
        .AddOpenApiServices(config, env)
        .AddGraphQLConfig(apiConfig, env)
        .AddFileWriter(apiConfig, env)
        .AddHangfireServices(config, apiConfig, env);

    services.AddSignalR();
    services.AddRouting(o => o.LowercaseUrls = true);

    services.AddRateLimiter(opt =>
    {
        opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        opt.AddPolicy(
            "fixed-by-ip",
            httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Request.Headers["X-Forwarder-For"].ToString(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                    }
                )
        );

        opt.AddTokenBucketLimiter(
            "token",
            options =>
            {
                options.TokenLimit = 100;
                options.ReplenishmentPeriod = TimeSpan.FromSeconds(5);
                options.TokensPerPeriod = 100;
                options.AutoReplenishment = true;
            }
        );
    });
}

namespace Muzonia
{
    public partial class Program { }
}
