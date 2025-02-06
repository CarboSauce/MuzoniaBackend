using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.Playlists;

public class AddEntry : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/{id}/add/{trackId}", Handle);

    public record Response(
        EntityId Id,
        EntityId TrackId,
        EntityId PlaylistId,
        DateTime CreationDate,
        int Index
    );

    private static async Task<Results<Ok<Response>, BadRequest>> Handle(
        EntityId id,
        EntityId trackId,
        ApiDbContext dbContext
    )
    {
        var playlist = await dbContext.Playlists.FirstOrDefaultAsync(e =>
            e.Id == id
        );

        if (playlist is null)
        {
            return TypedResults.BadRequest();
        }

        var track = await dbContext.Tracks.FirstOrDefaultAsync(e =>
            e.Id == trackId && e.DataUri != null
        );

        if (track is null)
        {
            return TypedResults.BadRequest();
        }

        var entry = new PlaylistTrack
        {
            PlaylistId = id,
            TrackId = trackId,
            Index = playlist.TrackCount,
        };

        dbContext.PlaylistTracks.Add(entry);
        playlist.TrackCount += 1;

        await dbContext.SaveChangesAsync();

        return TypedResults.Ok(
            new Response(
                entry.Id,
                entry.TrackId,
                entry.PlaylistId,
                entry.CreationDate,
                entry.Index
            )
        );
    }
}
