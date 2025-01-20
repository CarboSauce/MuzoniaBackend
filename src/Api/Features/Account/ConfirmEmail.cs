using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.Account;

public class ConfirmEmail : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/confirmEmail", Handle)
            .WithName("ConfirmEmail")
            .WithDescription("Confirm email");

    private static async Task<
        Results<Created, BadRequest<IEnumerable<IdentityError>>>
    > Handle(
        SignInManager<AppUser> signInManager,
        EntityId userId,
        string token
    )
    {
        var user = await signInManager.UserManager.FindByIdAsync(
            userId.ToString()
        );

        if (user is null)
        {
            return TypedResults.BadRequest<IEnumerable<IdentityError>>(
                [new() { Description = "User not found" }]
            );
        }

        var result = await signInManager.UserManager.ConfirmEmailAsync(
            user,
            token
        );

        return result.Succeeded
            ? TypedResults.Created()
            : TypedResults.BadRequest(result.Errors);
    }
}
