using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Muzonia.Api.Features.Tracks;

public class GetArtistTracks : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id}/artist", Handle);

    public record Response(
        string Title,
        string Genre,
        Uri? DataUri,
        long Duration,
        EntityId Id,
        DateTime CreationDate,
        AlbumResponse Album,
        ArtistResponse PrimaryArtist,
        ArtistResponse[] OtherArtists
    );

    public record ArtistResponse(EntityId Id, string Name);

    public record AlbumResponse(EntityId Id, string Title, Uri ImageUri);

    private static async Task<Ok<Response[]>> Handle(
        EntityId id,
        ApiDbContext dbContext
    )
    {
        var track = await dbContext
            .TrackArtists.Where(e =>
                (e.ArtistId == id || e.Track.PrimaryArtistId == id)
                && e.Track.DataUri != null
            )
            .Select(e => new Response(
                e.Track.Title,
                e.Track.Genre,
                e.Track.DataUri,
                e.Track.Duration,
                e.Track.Id,
                e.Track.CreationDate,
                new(
                    e.Track.AlbumId,
                    e.Track.Album.Title,
                    e.Track.Album.ImageUri
                ),
                new(e.Track.PrimaryArtistId, e.Track.PrimaryArtist.Name),
                e.Track.Artists.Select(a => new ArtistResponse(a.Id, a.Name))
                    .ToArray()
            ))
            .ToArrayAsync();

        return TypedResults.Ok(track);
    }
}
