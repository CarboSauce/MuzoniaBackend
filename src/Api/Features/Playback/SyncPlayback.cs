using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Playback;

public class SyncPlayback : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/sync", Handle);

    public record Request(
        int CurrentIndex,
        int Timestamp,
        int Volume,
        bool IsPlaying,
        bool IsRepeat,
        bool IsRandom
    );

    private static async Task<Results<NoContent, BadRequest>> Handle(
        Request req,
        ApiDbContext dbContext,
        UserService userService
    )
    {
        var user = await userService.GetUser();

        var queue = await dbContext
            .CurrentQueues.Where(e => e.UserId == user.Id)
            .Where(e => e.Queue.OwnerId == user.Id)
            .Select(e => e.Queue)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(e => e.CurrentIndex, req.CurrentIndex)
                    .SetProperty(e => e.Timestamp, req.Timestamp)
                    .SetProperty(e => e.Volume, req.Volume)
                    .SetProperty(e => e.IsPlaying, req.IsPlaying)
                    .SetProperty(e => e.IsRepeat, req.IsRepeat)
                    .SetProperty(e => e.IsRandom, req.IsRandom)
            );

        return queue == 0
            ? TypedResults.BadRequest()
            : TypedResults.NoContent();
    }
}
