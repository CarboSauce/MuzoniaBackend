using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Muzonia.Core.Dto.Request;
using Muzonia.DbEf.Entities;

namespace Muzonia.Core.Services.Api;

public class UserService(
    IFileWriter fileWriter,
    UserManager<AppUser> userManager,
    ClaimsPrincipal claims
) : ITransient
{
    public virtual Task<AppUser?> GetUserInfo()
    {
        return userManager.GetUserAsync(claims);
    }

    public virtual async Task<AppUser> GetUser()
    {
        var user = await userManager.GetUserAsync(claims);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        return user;
    }

    public virtual async Task<AppUser?> GetUserById(EntityId id) =>
        await userManager.FindByIdAsync(id.ToString());

    public virtual async Task<(AppUser user, bool isAdmin)> CurrentUser()
    {
        var user = await userManager.GetUserAsync(claims);
        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }
        var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
        return (user, isAdmin);
    }

    public virtual Task<bool> IsUserAdmin(AppUser user)
    {
        return userManager.IsInRoleAsync(user, "Admin");
    }

    public virtual async Task<bool> IsNotAdmin(AppUser user)
    {
        return !await userManager.IsInRoleAsync(user, "Admin");
    }

    public virtual async Task DeleteUser()
    {
        var user = await userManager.GetUserAsync(claims);

        if (user is not null)
        {
            await userManager.DeleteAsync(user);
        }
    }

    public virtual async Task<AppUser?> EditUserInfo(EditUserRequest request)
    {
        var user = await userManager.GetUserAsync(claims);

        if (user is null)
        {
            return null;
        }

        if (request.Email is not null)
            user.Email = request.Email;

        // if (request.UserName is not null)
        //     user.UserName = request.UserName;
        //
        // if (request.File is not null)
        // {
        //     user.AvatarUri = await fileWriter.WriteAsync(
        //         request.File,
        //         "images/",
        //         Guid.NewGuid().ToString()
        //     );
        // }

        var result = await userManager.UpdateAsync(user);

        return result.Succeeded ? user : null;
    }
}
