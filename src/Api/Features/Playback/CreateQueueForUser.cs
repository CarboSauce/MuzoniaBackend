using Microsoft.AspNetCore.Http.HttpResults;
using Muzonia.Core.Services.Api;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.Playback;

public class CreateQueueForUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/create", Handle);

    private static async Task<Results<Ok, BadRequest<string>>> Handle(
        ApiDbContext dbContext,
        UserService userService
    )
    {
        var user = await userService.GetUser();
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
                QueueId = queue.Id,
                UserId = user.Id,
                IsBanned = false,
            }
        );
        dbContext.CurrentQueues.Add(
            new CurrentQueue { UserId = user.Id, QueueId = queue.Id, }
        );
        await dbContext.SaveChangesAsync();

        return TypedResults.Ok();
    }
}
