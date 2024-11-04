using Microsoft.AspNetCore.Identity;
using Muzonia.Core.Common;
using Muzonia.DbEf.Entities;

namespace Muzonia.Core.Services;

public class SeedAdmin(
    UserManager<AppUser> userManager,
    AdminConfig adminConfig
)
{
    public async Task SeedAsync()
    {
        var admin = new AppUser
        {
            UserName = adminConfig.UserName,
            Email = adminConfig.Email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, adminConfig.Password);

        var role = await userManager.GetRolesAsync(admin);
        if (role.Count == 0)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        await userManager.UpdateAsync(admin);
    }
}
