using Microsoft.AspNetCore.Identity;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Common;

public static class UserUtils
{
    public static Task<bool> IsUserAdmin(
        this UserManager<AppUser> userManager,
        AppUser user
    )
    {
        return userManager.IsInRoleAsync(user, "Admin");
    }
}
