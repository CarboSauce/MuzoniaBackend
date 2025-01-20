using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Muzonia.Api.Features.Artist;

public class GetArtistTracks : IEndpoint
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
        EntityId artistId,
        ApiDbContext dbContext
    )
    {
        var track = await dbContext
            .TrackArtists.Where(e =>
                e.ArtistId == artistId || e.Track.DataUri != null
            )
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

        return TypedResults.Ok(track);
    }
}
