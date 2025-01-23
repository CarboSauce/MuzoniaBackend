using Microsoft.AspNetCore.Http.HttpResults;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.User;

public class GetUserBasicInfo : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/basic", Handle)
            .WithName("GetUserBasicInfo")
            .WithDescription("Get user basic info");
    }

    public record Response(
        EntityId Id,
        string Username,
        string Email,
        DateTime CreationDate,
        bool IsAdmin,
        Uri? Avatar
    );

    private static async Task<Results<Ok<Response>, NotFound>> Handle(
        UserService userService
    )
    {
        var user = await userService.GetUserInfo();
        if (user is null)
        {
            return TypedResults.NotFound();
        }

        var isAdmin = await userService.IsUserAdmin(user);

        return TypedResults.Ok(
            new Response(
                user.Id,
                user.UserName!,
                user.Email!,
                user.CreationDate,
                isAdmin,
                user.AvatarUri
            )
        );
    }
}
