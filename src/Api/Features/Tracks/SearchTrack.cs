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
        EntityId AlbumId,
        EntityId PrimaryArtistId,
        EntityId[] OtherArtistIds
    );

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
            .Select(e => new Response(
                e.Title,
                e.Genre,
                e.DataUri,
                e.Duration,
                e.Id,
                e.CreationDate,
                e.AlbumId,
                e.PrimaryArtistId,
                e.Artists.Select(a => a.Id).ToArray()
            ))
            .Take(50)
            .ToArrayAsync();

        return TypedResults.Ok(track);
    }
}
