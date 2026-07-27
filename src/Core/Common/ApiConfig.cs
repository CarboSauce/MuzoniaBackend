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
    public string FfmpegPath { get; set; } = "ffmpeg";
    public string SenderEmailAddress { get; set; } = "";
    public string ApiDomain { get; set; }
    public string ApiPort { get; set; }
    public string ApiProtocol { get; set; }

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

public class EmailConfig
{
    public string ClientUrl { get; set; } = "http://localhost:3000";
    public string ConfirmEmailEndpoint { get; set; } = "confirmEmail";
    public string ForgotPasswordEndpoint { get; set; } = "forgotPassword";

    public static EmailConfig? Configure(
        IServiceCollection services,
        IConfiguration config
    )
    {
        var configurationSection = config.GetSection("EmailConfig");
        var emailConfig = configurationSection.Get<EmailConfig>();

        if (emailConfig is null)
            return null;

        services.Configure<EmailConfig>(configurationSection);
        services.AddSingleton(emailConfig);

        return emailConfig;
    }
}

public static class ConfigExt
{
    public static (
        ApiConfig? apiConfig,
        AdminConfig? adminConfig,
        EmailConfig? emailConfig
    ) AddConfigNullable(this IServiceCollection services, IConfiguration config)
    {
        var apiConfig = ApiConfig.Configure(services, config);
        var adminConfig = AdminConfig.Configure(services, config);
        var emailConfig = EmailConfig.Configure(services, config);

        return (apiConfig, adminConfig, emailConfig);
    }

    public static (
        ApiConfig apiConfig,
        AdminConfig adminConfig,
        EmailConfig emailConfig
    ) AddConfig(this IServiceCollection services, IConfiguration config)
    {
        var (apiConfig, adminConfig, emailConfig) = services.AddConfigNullable(
            config
        );

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

        if (emailConfig is null)
        {
            Console.WriteLine(
                "EmailConfig is missing, Creating default instance"
            );
        }

        return (apiConfig ?? new(), adminConfig ?? new(), emailConfig ?? new());
    }
}

public static class WebHostExtensions
{
    public static void AddConfig(this WebApplicationBuilder builder)
    {
        var cfg = builder.Configuration;
        var context = builder.Environment.EnvironmentName;
        cfg.Sources.Clear();

        var curdir = Directory.GetParent(AppContext.BaseDirectory)?.FullName;
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
