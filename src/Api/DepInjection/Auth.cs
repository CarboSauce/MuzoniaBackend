using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        var data = services.AddAuthorization();
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddExternalCookie();

        // services
        //     .AddIdentityCore<AppUser>(o =>
        //     {
        //         o.User.RequireUniqueEmail = true;
        //
        //         o.SignIn.RequireConfirmedEmail = !apiConfig.UseNoopEmail;
        //
        //         if (env.IsDevelopment())
        //         {
        //             o.Password.RequireDigit = false;
        //             o.Password.RequireLowercase = false;
        //             o.Password.RequireUppercase = false;
        //             o.Password.RequireNonAlphanumeric = false;
        //             o.Password.RequiredLength = 0;
        //         }
        //         else
        //         {
        //             o.Password.RequireDigit = true;
        //             o.Password.RequireLowercase = true;
        //             o.Password.RequireUppercase = true;
        //             o.Password.RequireNonAlphanumeric = true;
        //             o.Password.RequiredLength = 8;
        //             o.SignIn.RequireConfirmedEmail = true;
        //         }
        //     })
        //     .AddDefaultTokenProviders()
        //     .AddSignInManager();

        return services;
    }
}
