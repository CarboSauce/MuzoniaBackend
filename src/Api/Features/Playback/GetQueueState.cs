using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Core.Services.Api;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.Playback;

public class GetQueueState : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{queueId}/state", Handle);

    public record Response(
        EntityId Id,
        EntityId OwnerId,
        bool IsPlaying,
        int Timestamp,
        bool IsRepeat,
        bool IsRandom,
        bool IsPublic,
        bool IsModifiable,
        bool IsCurrent,
        int CurrentIndex,
        QueueUsersResponse[] QueueUsers
    );

    public record QueueUsersResponse(EntityId Id, UserResponse User);

    public record UserResponse(EntityId Id, string Username, Uri? Avatar);

    public static async Task<Results<Ok<Response>, BadRequest>> Handle(
        EntityId queueId,
        ApiDbContext dbContext,
        UserService userService
    )
    {
        var user = await userService.GetUser();

        var playbackQueue = await dbContext
            .PlaybackQueues.Where(e =>
                e.Id == queueId
                && (
                    e.OwnerId == user.Id
                    || e.QueueUsers.Any(u => u.UserId == user.Id)
                )
            )
            .Select(e => new Response(
                e.Id,
                e.OwnerId,
                e.IsPlaying,
                e.Timestamp,
                e.IsRepeat,
                e.IsRandom,
                e.IsPublic,
                e.IsModifiable,
                dbContext.CurrentQueues.Any(q =>
                    q.UserId == user.Id && q.QueueId == e.Id
                ),
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

        return playbackQueue is not null
            ? TypedResults.Ok(playbackQueue)
            : TypedResults.BadRequest();
    }
}
