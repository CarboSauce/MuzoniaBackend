using Microsoft.AspNetCore.Identity;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Mutation;

public class LoginInput
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required bool RememberMe { get; set; }
}

public partial class Mutation
{
    public async Task<bool> Login(
        LoginInput request,
        [Service] SignInManager<AppUser> signInManager
    )
    {
        var result = await signInManager.PasswordSignInAsync(
            request.Username,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: false
        );

        if (!result.Succeeded)
        {
            throw new GraphQLException(
                ErrorBuilder
                    .New()
                    .SetMessage("Invalid login attempt.")
                    .SetCode("INVALID_LOGIN")
                    .Build()
            );
        }

        return result.Succeeded;
    }
}
