using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Playback;

public class GetCurrentQueue : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/current", Handle);

    public record Response(
        EntityId Id,
        EntityId OwnerId,
        bool IsPlaying,
        int Timestamp,
        bool IsRepeat,
        bool IsRandom,
        bool IsCurrent,
        bool IsModifiable,
        bool IsPublic,
        int CurrentIndex,
        QueueUsersResponse[] QueueUsers
    );

    public record QueueUsersResponse(EntityId Id, UserResponse User);

    public record UserResponse(EntityId Id, string Username, Uri? Avatar);

    private static async Task<Results<Ok<Response>, BadRequest>> Handle(
        ApiDbContext dbContext,
        UserService userService
    )
    {
        var user = await userService.GetUser();

        var queue = await dbContext
            .CurrentQueues.Where(e => e.UserId == user.Id)
            .Select(e => e.Queue)
            .Select(e => new Response(
                e.Id,
                e.OwnerId,
                e.IsPlaying,
                e.Timestamp,
                e.IsRepeat,
                e.IsRandom,
                true,
                e.IsModifiable,
                e.IsPublic,
                e.CurrentIndex,
                e.QueueUsers.Where(u => !u.IsBanned)
                    .Select(u => new QueueUsersResponse(
                        u.Id,
                        new UserResponse(
                            u.User.Id,
                            u.User.UserName!,
                            u.User.AvatarUri
                        )
                    ))
                    .ToArray()
            ))
            .FirstOrDefaultAsync();

        return queue is not null
            ? TypedResults.Ok(queue)
            : TypedResults.BadRequest();
    }
}
