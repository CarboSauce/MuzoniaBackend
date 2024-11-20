using System.Net.Mime;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Muzonia.Core.Common;

public class ApiConfig
{
    public bool AllowLocalhost { get; set; } = false;
    public bool UseStaticFiles { get; set; } = true;
    public string? StaticContentRoot { get; set; } = null;
    public bool UseNoopEmail { get; set; } = true;
    public string SenderEmailAddress { get; set; } = "";

    public static ApiConfig? Configure(
        IServiceCollection services,
        IConfiguration config
    )
    {
        var apiConfig = config.Get<ApiConfig>();

        if (apiConfig is null)
        {
            return null;
        }

        services.Configure<ApiConfig>(config);
        services.AddSingleton(apiConfig);

        return apiConfig;
    }
}

public class AdminConfig
{
    public string UserName { get; set; } = "admin";
    public string Email { get; set; } = "admin@admin";
    public string Password { get; set; } = "admin";

    public static AdminConfig? Configure(
        IServiceCollection services,
        IConfiguration config
    )
    {
        var configurationSection = config.GetSection("AdminConfig");
        var adminConfig = configurationSection.Get<AdminConfig>();

        if (adminConfig is null)
        {
            return null;
        }

        services.Configure<AdminConfig>(configurationSection);
        services.AddSingleton(adminConfig);

        return adminConfig;
    }
}

public class AspireConfig
{
    public bool UsePostgres { get; set; } = false;
    public bool UseRedis { get; set; } = false;
    public bool UseAspire { get; set; } = false;

    public static AspireConfig? Configure(
        IServiceCollection services,
        IConfiguration config
    )
    {
        var configurationSection = config.GetSection("Aspire");
        var aspireConfig = configurationSection.Get<AspireConfig>();

        if (aspireConfig is null)
        {
            return null;
        }

        services.Configure<AspireConfig>(configurationSection);
        services.AddSingleton(aspireConfig);

        return aspireConfig;
    }
}

public static class ConfigExt
{
    public static (
        ApiConfig? apiConfig,
        AdminConfig? adminConfig,
        AspireConfig? aspireConfig
    ) AddConfigNullable(this IServiceCollection services, IConfiguration config)
    {
        var apiConfig = ApiConfig.Configure(services, config);
        var adminConfig = AdminConfig.Configure(services, config);
        var aspireConfig = AspireConfig.Configure(services, config);

        return (apiConfig, adminConfig, aspireConfig);
    }

    public static (
        ApiConfig apiConfig,
        AdminConfig adminConfig,
        AspireConfig aspireConfig
    ) AddConfig(this IServiceCollection services, IConfiguration config)
    {
        (var apiConfig, var adminConfig, var aspireConfig) =
            services.AddConfigNullable(config);

        if (apiConfig is null)
        {
            Console.WriteLine(
                "ApiConfig is missing, Creating default instance"
            );
        }
        if (adminConfig is null)
        {
            Console.WriteLine(
                "AdminConfig is missing, Creating default instance"
            );
        }
        if (aspireConfig is null)
        {
            Console.WriteLine(
                "AspireConfig is missing, Creating default instance"
            );
        }

        return (
            apiConfig ?? new(),
            adminConfig ?? new(),
            aspireConfig ?? new()
        );
    }
}

public static class WebHostExtensions
{
    public static void AddConfig(this WebApplicationBuilder builder)
    {
        var cfg = builder.Configuration;
        var context = builder.Environment.EnvironmentName;
        cfg.Sources.Clear();

        var curdir = Directory
            .GetParent(Directory.GetCurrentDirectory())
            ?.FullName;
        ArgumentNullException.ThrowIfNull(curdir);

        cfg.SetBasePath(curdir)
            .AddJsonFile("appsettings.json")
            .AddJsonFile(
                $"appsettings.{context}.json",
                optional: true,
                reloadOnChange: true
            )
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetCallingAssembly());
    }
}
