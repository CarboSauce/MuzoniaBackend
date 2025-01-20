using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.Account;

public class ResetPassword : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/resetPassword", Handle);

    private static async Task<
        Results<Ok, NotFound, BadRequest<IEnumerable<IdentityError>>>
    > Handle(
        EntityId userId,
        string token,
        string newPassword,
        UserManager<AppUser> userManager
    )
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return TypedResults.NotFound();
        }

        var result = await userManager.ResetPasswordAsync(
            user,
            token,
            newPassword
        );

        if (!result.Succeeded)
        {
            return TypedResults.BadRequest(result.Errors);
        }

        return TypedResults.Ok();
    }
}
