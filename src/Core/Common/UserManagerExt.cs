using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Muzonia.DbEf.Entities;

namespace Muzonia.Core.Common;

public static class UserManagerExt
{
    public static async Task<AppUser> GetCurrentUser(
        this UserManager<AppUser> userManager,
        ClaimsPrincipal claims
    )
    {
        var user = await userManager.GetUserAsync(claims);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        return user;
    }
}
