using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.Account;

public class LoginUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/login", Handle).WithDescription("Login user");

    public record Request(string Username, string Password, bool RememberMe);

    private static async Task<Results<Ok, UnauthorizedHttpResult>> Handle(
        SignInManager<AppUser> signInManager,
        Request request
    )
    {
        var result = await signInManager.PasswordSignInAsync(
            request.Username,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: false
        );

        return result.Succeeded
            ? TypedResults.Ok()
            : TypedResults.Unauthorized();
    }
}
