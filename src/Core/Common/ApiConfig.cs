using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Muzonia.Core.Common;

public class ApiConfig
{
    public bool AllowLocalhost { get; set; } = false;
    public bool UseStaticFiles { get; set; } = true;
    public string? StaticContentRoot { get; set; } = null;

    public static void Configure(
        IServiceCollection services,
        IConfiguration config
    )
    {
        services.Configure<ApiConfig>(config);
    }
}

public class AdminConfig
{
    public string UserName { get; set; } = "admin";
    public string Email { get; set; } = "admin@admin";
    public string Password { get; set; } = "admin";

    public static void Configure(
        IServiceCollection services,
        IConfiguration config
    )
    {
        var configurationSection = config.GetSection("AdminConfig");
        services.Configure<AdminConfig>(configurationSection);
    }
}
