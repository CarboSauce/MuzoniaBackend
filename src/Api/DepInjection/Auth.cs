using Microsoft.AspNetCore.Identity;
using Muzonia.Core.Common;
using Muzonia.DbEf;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.DepInjection;

internal static class Auth
{
    public static IServiceCollection AddAuth(
        this IServiceCollection services,
        IConfiguration config,
        ApiConfig apiConfig,
        IWebHostEnvironment env
    )
    {
        services.AddAuthorization();
        services
            .AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(
                IdentityConstants.ApplicationScheme,
                o =>
                {
                    o.Cookie.SameSite = SameSiteMode.None;
                    o.Cookie.Name = "MuzoniaAuth";
                    o.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };

                    o.Events.OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    };
                }
            );

        services
            .AddIdentityCore<AppUser>(o =>
            {
                o.User.RequireUniqueEmail = true;

                o.SignIn.RequireConfirmedEmail = !apiConfig.UseNoopEmail;

                if (env.IsDevelopment())
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
                    o.SignIn.RequireConfirmedEmail = true;
                }
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApiDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager();

        return services;
    }
}
