using Hangfire;
using Hangfire.AspNetCore;
using Hangfire.Common;
using Hangfire.PostgreSql;
using Muzonia.Core.Common;

namespace Muzonia.Api.DepInjection;

public class HangfireJobActivator : JobActivator
{
    private readonly IServiceProvider _serviceProvider;

    public HangfireJobActivator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override object ActivateJob(Type jobType)
    {
        return _serviceProvider.GetRequiredService(jobType);
    }
}

public static class Hangfire
{
    public static IServiceCollection AddHangfireServices(
        this IServiceCollection services,
        IConfiguration config,
        ApiConfig apiConfig,
        IWebHostEnvironment env
    )
    {
        services.AddHangfire(c =>
        {
            c.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(o =>
                    o.UseNpgsqlConnection(config.GetConnectionString("apidb"))
                );
        });
        services.AddHangfireServer(o =>
        {
            o.WorkerCount = 1;
            o.HeartbeatInterval = TimeSpan.FromSeconds(3);
            o.SchedulePollingInterval = TimeSpan.FromSeconds(3);
        });
        return services;
    }

    public static IApplicationBuilder UseHangfire(
        this IApplicationBuilder app,
        IWebHostEnvironment env
    )
    {
        if (!env.IsDevelopment())
        {
            return app;
        }
        app.UseHangfireDashboard();
        return app;
    }
}
