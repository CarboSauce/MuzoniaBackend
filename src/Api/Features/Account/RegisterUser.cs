using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Muzonia.Core;
using Muzonia.Core.Services;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.Account;

public class RegisterUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/register", Handle);

    public record Request(string Username, string Email, string Password);

    public record Response(
        string Username,
        string Email,
        DateTime CreationDate
    );

    private static async Task<
        Results<Ok<Response>, BadRequest<IEnumerable<IdentityError>>>
    > Handle(
        HttpContext context,
        SignInManager<AppUser> signInManager,
        [FromServices] LinkGenerator linkGenerator,
        IEmail email,
        [FromBody] Request request
    )
    {
        var user = new AppUser
        {
            UserName = request.Username,
            Email = request.Email
        };

        var result = await signInManager.UserManager.CreateAsync(
            user,
            request.Password
        );

        if (!result.Succeeded)
        {
            return TypedResults.BadRequest(result.Errors);
        }

        // Generate Token and send email
        var token =
            await signInManager.UserManager.GenerateEmailConfirmationTokenAsync(
                user
            );

        if (email is NoopEmail)
        {
            await signInManager.UserManager.ConfirmEmailAsync(user, token);
        }
        else
        {
            var callbackUrl = linkGenerator.GetUriByName(
                context,
                "ConfirmEmail",
                new { userId = user.Id, token }
            );

            await email.SendEmailAsync(
                user.Email,
                "Confirm your email",
                $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>."
            );
        }

        return TypedResults.Ok(
            new Response(user.UserName, user.Email, user.CreationDate)
        );
    }
}
