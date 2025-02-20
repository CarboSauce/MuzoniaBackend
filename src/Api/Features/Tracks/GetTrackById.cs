using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Muzonia.Api.Features.Tracks;

public class GetTrackById : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id}", Handle);

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

    public static async Task<Results<Ok<Response>, BadRequest>> Handle(
        EntityId id,
        ApiDbContext dbContext
    )
    {
        var track = await dbContext
            .Tracks.Where(e => e.Id == id && e.DataUri != null)
            .Select(e => new Response(
                e.Title,
                e.Genre,
                e.DataUri,
                e.Duration,
                e.Id,
                e.CreationDate,
                new(e.AlbumId, e.Album.Title, e.Album.ImageUri),
                new(e.PrimaryArtistId, e.PrimaryArtist.Name),
                e.Artists.Select(a => new ArtistResponse(a.Id, a.Name))
                    .ToArray()
            ))
            .FirstOrDefaultAsync();

        if (track is null)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(track);
    }
}
