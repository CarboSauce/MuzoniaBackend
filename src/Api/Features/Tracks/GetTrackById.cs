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
        EntityId AlbumId,
        EntityId PrimaryArtistId,
        EntityId[] OtherArtistIds
    );

    private static async Task<Results<Ok<Response>, BadRequest>> Handle(
        EntityId id,
        ApiDbContext dbContext
    )
    {
        var track = await dbContext
            .Tracks.Where(e => e.Id == id)
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
            .FirstOrDefaultAsync();

        if (track is null)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(track);
    }
}
