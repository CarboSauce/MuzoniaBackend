using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.Account;

public class ConfirmEmail : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/confirmEmail", Handle)
            .WithName("ConfirmEmail")
            .WithDescription("Confirm email");

    public record Request(EntityId UserId, string Token);

    private static async Task<
        Results<Created, BadRequest<IEnumerable<IdentityError>>>
    > Handle(SignInManager<AppUser> signInManager, [FromBody] Request req)
    {
        var (userId, token) = req;

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
