using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Muzonia.Core.Services.Api;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.User;

public class DeleteUserById : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/{id}", Handle);

    private static async Task<
        Results<NoContent, NotFound, ForbidHttpResult>
    > Handle(
        EntityId id,
        UserManager<AppUser> userManager,
        UserService userService
    )
    {
        var (curUser, isAdmin) = await userService.CurrentUser();

        if (curUser.Id != id && !isAdmin)
        {
            return TypedResults.Forbid();
        }

        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return TypedResults.NotFound();
        }

        await userManager.DeleteAsync(user);
        return TypedResults.NoContent();
    }
}
