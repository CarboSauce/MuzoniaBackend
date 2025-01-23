using System.Text.RegularExpressions;

namespace Muzonia.Api.DepInjection;

internal static class Cors
{
    public const string CorsPolicyName = "MuzoniaCors";

    public static IServiceCollection AddCustomCors(
        this IServiceCollection services,
        IConfiguration cfg
    ) =>
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policyBuilder =>
            {
                var allowedOrigins = cfg.GetSection("CorsAllowedOrigins")
                    .Get<string[]>();

                policyBuilder
                    .AllowCredentials()
                    .WithOrigins(allowedOrigins ?? ["http://localhost:3000"])
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
}
