using Microsoft.AspNetCore.Http.HttpResults;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.User;

public class GetUserInfo : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/", Handle)
            .WithName("GetUserInfo")
            .WithDescription("Get user info");
    }

    private static async Task<Results<Ok<UserResponse>, NotFound>> Handle(
        HttpContext context,
        UserService userService
    )
    {
        var user = await userService.GetUserInfo();

        return user is not null
            ? TypedResults.Ok(
                UserResponse.From(
                    user,
                    user.Artist is null ? null : new(user.Artist)
                )
            )
            : TypedResults.NotFound();
    }
}