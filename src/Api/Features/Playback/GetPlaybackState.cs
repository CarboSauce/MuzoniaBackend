using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Core.Services.Api;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.Playback;

public class GetPlaybackState : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/state", Handle);

    public record Response(
        EntityId Id,
        bool IsPlaying,
        int Volume,
        int Timestamp,
        bool IsRepeat,
        bool IsRandom,
        int CurrentIndex,
        DeviceResponse? Device
    );

    public record DeviceResponse(EntityId Id, string Name);

    private static async Task<Results<Ok<Response>, BadRequest>> Handle(
        ApiDbContext dbContext,
        UserService userService
    )
    {
        var user = await userService.GetUser();

        var playbackQueue = await dbContext
            .PlaybackQueues.Where(p => p.Id == user.Id)
            .Select(p => new
            {
                p.Id,
                p.IsPlaying,
                p.Volume,
                p.Timestamp,
                p.IsRepeat,
                p.IsRandom,
                p.CurrentIndex,
                Device = p.Device != null
                    ? new { p.Device.Id, p.Device.Name, }
                    : null,
            })
            .FirstOrDefaultAsync();

        if (playbackQueue is null)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(
            new Response(
                playbackQueue.Id,
                playbackQueue.IsPlaying,
                playbackQueue.Volume,
                playbackQueue.Timestamp,
                playbackQueue.IsRepeat,
                playbackQueue.IsRandom,
                playbackQueue.CurrentIndex,
                playbackQueue.Device != null
                    ? new DeviceResponse(
                        playbackQueue.Device.Id,
                        playbackQueue.Device.Name
                    )
                    : null
            )
        );
    }
}
