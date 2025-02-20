using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Playback;

public class GetUsers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{queueId}/users", Handle);

    public record Response(EntityId Id, UserResponse User);

    public record UserResponse(EntityId Id, string Username, Uri? Avatar);

    public static async Task<Results<Ok<Response[]>, BadRequest>> Handle(
        EntityId queueId,
        UserService userService,
        ApiDbContext dbContext
    )
    {
        var user = await userService.GetUser();

        var result = await dbContext
            .QueueUsers.Where(e => e.QueueId == queueId && !e.IsBanned)
            .Select(e => new Response(
                e.UserId,
                new UserResponse(e.UserId, e.User.UserName!, e.User.AvatarUri)
            ))
            .ToArrayAsync();
        if (result.All(e => e.User.Id != user.Id))
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(result);
    }
}
