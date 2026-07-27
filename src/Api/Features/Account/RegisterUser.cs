using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Muzonia.Core.Common;
using Muzonia.Core.Services;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.Account;

public class RegisterUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/register", Handle).WithValidation<Request>();

    public record Request(string Username, string Email, string Password);

    public record Response(
        string Username,
        string Email,
        DateTime CreationDate
    );

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Username).NotEmpty().Length(3, 64);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        }
    }

    public static async Task<
        Results<Ok<Response>, BadRequest<IEnumerable<IdentityError>>>
    > Handle(
        [FromBody] Request request,
        UserManager<AppUser> userManager,
        ApiDbContext dbContext,
        IEmail email,
        IHostEnvironment env,
        IOptions<EmailConfig> emailConfig
    )
    {
        var user = new AppUser
        {
            UserName = request.Username,
            Email = request.Email,
        };
        var hasUser = await userManager.Users.AnyAsync();

        var result = await dbContext.UseTransactionAsync(async () =>
        {
            var identityResult = await userManager.CreateAsync(
                user,
                request.Password
            );

            if (!identityResult.Succeeded)
            {
                return identityResult;
            }

            var queue = new PlaybackQueue
            {
                OwnerId = user.Id,
                IsRepeat = false,
                IsPlaying = false,
                IsRandom = false,
                CurrentIndex = 0,
                TrackCount = 0,
                Timestamp = 0,
                IsModifiable = false,
                IsPublic = false,
            };

            dbContext.PlaybackQueues.Add(queue);

            dbContext.QueueUsers.Add(
                new QueueUser
                {
                    UserId = user.Id,
                    QueueId = queue.Id,
                    IsBanned = false,
                }
            );

            dbContext.CurrentQueues.Add(
                new CurrentQueue { UserId = user.Id, QueueId = queue.Id }
            );

            await dbContext.SaveChangesAsync();

            return identityResult;
        });

        if (!result.Succeeded)
        {
            return TypedResults.BadRequest(result.Errors);
        }

        if (env.IsDevelopment() && !hasUser)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }

        // Generate Token and send email
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        if (email is NoopEmail)
        {
            await userManager.ConfirmEmailAsync(user, token);
        }
        else
        {
            var config = emailConfig.Value;

            var callbackUri = new UriBuilder($"{config.ClientUrl}")
            {
                Path = config.ConfirmEmailEndpoint,
                Query = $"token={Uri.EscapeDataString(token)}&userId={user.Id}",
            };

            await email.SendEmailAsync(
                user.Email,
                "Confirm your email",
                $"Please confirm your account by <a clicktracking=\"off\" href='{callbackUri.Uri}'>clicking here</a>."
            );
        }

        return TypedResults.Ok(
            new Response(user.UserName, user.Email, user.CreationDate)
        );
    }
}
