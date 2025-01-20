using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Album;

public class GetAlbumTracks : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id}/tracks", Handle);

    public record Response(
        EntityId Id,
        string Title,
        string Genre,
        Uri? DataUri,
        long Duration,
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
        if (!context.IsLoggedIn())
        {
            return TypedResults.BadRequest();
        }

        var tracks = await dbContext
            .Tracks.Where(e => e.AlbumId == id)
            .Select(e => new Response(
                e.Id,
                e.Title,
                e.Genre,
                e.DataUri,
                e.Duration,
                e.CreationDate,
                e.AlbumId,
                e.PrimaryArtistId,
                e.Artists.Select(a => a.Id).ToArray()
            ))
            .ToArrayAsync();

        return TypedResults.Ok(tracks);
    }
}
