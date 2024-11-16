using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Muzonia.Core.Services;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController(
    SignInManager<AppUser> signInManager,
    IEmail email
) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IResult> Register([FromBody] RegisterRequest request)
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
            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "Account",
                new { email = user.Email, token },
                Request.Scheme
            );

            await email.SendEmailAsync(
                user.Email,
                "Confirm your email",
                $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>."
            );
        }

        return result.Succeeded ? Results.Ok() : Results.BadRequest();
    }

    [HttpPost("confirmEmail")]
    public async Task<IResult> ConfirmEmail(
        [FromQuery] string email,
        [FromQuery] string token
    )
    {
        var user = await signInManager.UserManager.FindByEmailAsync(email);

        if (user is null)
        {
            return Results.NotFound();
        }

        var result = await signInManager.UserManager.ConfirmEmailAsync(
            user,
            token
        );

        return result.Succeeded ? Results.Ok() : Results.BadRequest();
    }

    [HttpPost("login")]
    public async Task<IResult> Login([FromBody] LoginRequest request)
    {
        var result = await signInManager.PasswordSignInAsync(
            request.Username,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: false
        );

        if (result.Succeeded)
        {
            return Results.Ok();
        }

        return Results.Unauthorized();
    }

    [HttpPost("logout")]
    public async Task<IResult> Logout()
    {
        await signInManager.SignOutAsync();

        return Results.Ok();
    }
}

public record RegisterRequest(string Username, string Email, string Password);

public record ConfirmEmailRequest(string Email, string Token);

public record LoginRequest(string Username, string Password, bool RememberMe);
