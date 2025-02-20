using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Muzonia.Core.Common;
using Muzonia.Core.Services;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.Account;

public class ResetPasswordRequest : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/resetPasswordRequest", Handle).WithValidation<Request>();

    public record Request(string Email);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Email).EmailAddress();
        }
    }

    public static async Task<IResult> Handle(
        Request req,
        UserManager<AppUser> userManager,
        IEmail email,
        IOptions<EmailConfig> emailConfig
    )
    {
        var user = await userManager.FindByEmailAsync(req.Email);

        if (user?.Email is null)
        {
            return TypedResults.BadRequest();
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        // Send email with token
        if (email is NoopEmail)
        {
            return TypedResults.Ok(new { UserId = user.Id, Token = token });
        }

        var config = emailConfig.Value;
        var callbackUri = new UriBuilder($"{config.ClientUrl}")
        {
            Path = config.ForgotPasswordEndpoint,
            Query = $"token={Uri.EscapeDataString(token)}&userId={user.Id}",
        };

        await email.SendEmailAsync(
            user.Email,
            "Reset your password",
            $"You can reset your password by clicking the link. <a clicktracking=\"off\" href='{callbackUri.Uri}'>Clicking here</a>."
        );

        return TypedResults.Ok();
    }
}
