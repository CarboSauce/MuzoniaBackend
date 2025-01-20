using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.User;

public class DeleteUserById : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/{id}", Handle);

    [Authorize(Roles = "Admin")]
    private static async Task<Results<NoContent, NotFound>> Handle(
        EntityId id,
        UserManager<AppUser> userManager
    )
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return TypedResults.NotFound();
        }

        await userManager.DeleteAsync(user);
        return TypedResults.NoContent();
    }
}
