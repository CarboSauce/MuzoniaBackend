using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Muzonia.Api.Features.Tracks;

public class IsTranscoded : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id}/transcoding", Handle);

    public record Request();

    public record Response(bool IsFinished);

    private static async Task<Results<Ok<Response>, BadRequest>> Handle(
        EntityId id,
        ApiDbContext dbContext
    )
    {
        var track = await dbContext
            .Tracks.Where(e => e.Id == id)
            .Select(e => new Response(e.DataUri == null))
            .FirstOrDefaultAsync();

        return track is not null
            ? TypedResults.Ok(new Response(track.IsFinished))
            : TypedResults.BadRequest();
    }
}
