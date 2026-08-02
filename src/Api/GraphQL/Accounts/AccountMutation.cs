using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Muzonia.Core.Common;
using Muzonia.Core.Services;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Accounts;

[MutationType]
public static class AccountMutation
{
    [Error(typeof(UserNotFoundError))]
    public static async Task<FieldResult<bool>> LoginAsync(
        string username,
        string password,
        bool rememberMe,
        SignInManager<AppUser> signInManager
    )
    {
        var result = await signInManager.PasswordSignInAsync(
            username,
            password,
            rememberMe,
            false
        );

        return result.Succeeded
            ? true
            : new FieldResult<bool>(new UserNotFoundError(username));
    }

    public static async Task<bool> LogoutAsync(
        SignInManager<AppUser> signInManager
    )
    {
        await signInManager.SignOutAsync();
        return true;
    }

    public static async Task<RegisterPayload> RegisterAsync(
        RegisterInput request,
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
            throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder
                        .New()
                        .SetMessage(e.Description)
                        .SetCode(e.Code)
                        .Build()
                )
            );
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

        return new RegisterPayload(
            user.UserName,
            user.Email,
            user.CreationDate
        );
    }
}

public record RegisterInput(
    [property: MinLength(3)] string Username,
    [property: MinLength(8)] string Password,
    [property: EmailAddress] string Email
);

public record RegisterPayload(
    string Username,
    string Email,
    DateTime CreationDate
);

public record UserNotFoundError(string Username)
{
    public string Message => $"User with username {Username} not found";
}
