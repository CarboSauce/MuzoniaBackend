using Microsoft.AspNetCore.Http.HttpResults;
using Muzonia.Core.Dto.Request;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.User;

public class EditUserInfo : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/editUserInfo", Handle)
            .WithName("EditUserInfo")
            .WithDescription("Edit user info");
    }

    private static async Task<Results<Ok<UserResponse>, BadRequest>> Handle(
        HttpContext context,
        UserService userService,
        EditUserRequest request
    )
    {
        var user = await userService.EditUserInfo(request);

        ArtistResponse? artist = user?.Artist is null ? null : new(user.Artist);

        return user is not null
            ? TypedResults.Ok(UserResponse.From(user, artist))
            : TypedResults.BadRequest();
    }
}
