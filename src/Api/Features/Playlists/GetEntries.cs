using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;

namespace Muzonia.Api.Features.Playlists;

public class GetEntries : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id}/tracks", Handle);

    public record Response(
        string Title,
        string Genre,
        Uri? DataUri,
        long Duration,
        EntityId Id,
        DateTime CreationDate,
        EntityId AlbumId,
        EntityId PrimaryArtistId,
        EntityId[] OtherArtistIds
    );

    private static async Task<Results<Ok<Response[]>, BadRequest>> Handle(
        EntityId id,
        ApiDbContext dbContext,
        HttpContext context
    )
    {
        var userId = context.GetUserId();

        var playlist = await dbContext.Playlists.AnyAsync(e =>
            e.Id == id && (e.UserId == userId || e.IsPublic)
        );

        if (playlist is false)
        {
            return TypedResults.BadRequest();
        }

        var tracks = await dbContext
            .PlaylistTracks.Where(e => e.PlaylistId == id)
            .Select(e => new Response(
                e.Track.Title,
                e.Track.Genre,
                e.Track.DataUri,
                e.Track.Duration,
                e.Track.Id,
                e.Track.CreationDate,
                e.Track.AlbumId,
                e.Track.PrimaryArtistId,
                e.Track.Artists.Select(a => a.Id).ToArray()
            ))
            .ToArrayAsync();

        return TypedResults.Ok(tracks);
    }
}
