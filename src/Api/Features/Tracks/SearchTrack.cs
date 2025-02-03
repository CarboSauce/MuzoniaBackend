using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Muzonia.Api.Features.Tracks;

public class SearchTrack : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/search/{name}", Handle);

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

    private static async Task<Results<Ok<Response[]>, BadRequest>> Handle(
        string name,
        ApiDbContext dbContext
    )
    {
        var searchName = $"%{name}%";
        var track = await dbContext
            .Tracks.Where(e =>
                EF.Functions.ILike(e.Title, searchName)
                || EF.Functions.ILike(e.Album.Title, searchName)
                || EF.Functions.ILike(e.Genre, searchName)
                || e.Artists.Any(a => EF.Functions.ILike(a.Name, searchName))
            )
            .Where(e => e.DataUri != null)
            .OrderBy(a => a.Id)
            .Take(50)
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
            .ToArrayAsync();

        return TypedResults.Ok(track);
    }
}
