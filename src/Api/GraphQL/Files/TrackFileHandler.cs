using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Services;
using Muzonia.Core.Services.Transcoding;

namespace Muzonia.Api.GraphQL.Files;

public class TrackFileHandler : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/track", Handle);
    }

    public record TrackUploadResponse(string transcodingId);

    public static async Task<
        Results<Ok<TrackUploadResponse>, BadRequest<string>>
    > Handle(
        EntityId trackId,
        [FromForm] IFormFile file,
        ClaimsPrincipal claims,
        ApiDbContext dbContext,
        IDbFileService fileService,
        ITranscodingService transcodingService,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;

        var artist = await dbContext
            .Users.GetArtist(userId)
            .FirstOrDefaultAsync(ct);

        if (artist is null)
        {
            return TypedResults.BadRequest("User is not an artist");
        }

        if (
            !await dbContext.TrackArtists.AnyAsync(
                ta => ta.ArtistId == artist.Id && ta.TrackId == trackId,
                ct
            )
        )
        {
            return TypedResults.BadRequest(
                "Track doesn't belong to this artist"
            );
        }

        var newFile = await fileService.SaveFile(file);

        dbContext.Files.Add(newFile);
        await dbContext.SaveChangesAsync(ct);

        var transcodeId = await transcodingService.Enqueue(trackId, newFile.Id);

        return TypedResults.Ok(new TrackUploadResponse(transcodeId));
    }
}
