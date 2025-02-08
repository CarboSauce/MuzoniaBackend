using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.Account;

public class ResetPassword : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/resetPassword", Handle);

    public record Request(EntityId UserId, string Token, string NewPassword);

    private static async Task<
        Results<Ok, NotFound, BadRequest<IEnumerable<IdentityError>>>
    > Handle(Request req, UserManager<AppUser> userManager)
    {
        var user = await userManager.FindByIdAsync(req.UserId.ToString());
        if (user is null)
        {
            return TypedResults.NotFound();
        }

        var result = await userManager.ResetPasswordAsync(
            user,
            req.Token,
            req.NewPassword
        );

        if (!result.Succeeded)
        {
            return TypedResults.BadRequest(result.Errors);
        }

        return TypedResults.Ok();
    }
}
