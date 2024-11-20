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
    public async Task<AppUser?> GetUserInfo()
    {
        var user = await userManager.GetUserAsync(claims);

        return user;
    }

    public async Task<AppUser?> EditUserInfo(EditUserRequest request)
    {
        var user = await userManager.GetUserAsync(claims);

        if (user is null)
        {
            return null;
        }

        if (request.Email is not null)
            user.Email = request.Email;

        if (request.UserName is not null)
            user.UserName = request.UserName;

        if (request.File is not null)
        {
            user.AvatarUri = await fileWriter.WriteAsync(
                request.File,
                "images/"
            );
        }

        var result = await userManager.UpdateAsync(user);

        return result.Succeeded ? user : null;
    }
}
