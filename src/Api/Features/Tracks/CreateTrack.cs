using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Common;
using Muzonia.Core.Services;
using Muzonia.Core.Services.Transcoding;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.Tracks;

public class CreateTrack : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", Handle);

    public record Request(
        string Title,
        string Genre,
        IFormFile File,
        EntityId AlbumId,
        EntityId[] OtherArtists
    );

    public record Response(
        string Title,
        string Genre,
        EntityId Id,
        DateTime CreationDate,
        EntityId AlbumId,
        EntityId PrimaryArtistId,
        EntityId[] OtherArtistIds,
        string TranscodingId
    );

    private static async Task<Results<Ok<Response>, BadRequest<string>>> Handle(
        [FromForm] Request request,
        ClaimsPrincipal claims,
        ApiDbContext dbContext,
        IDbFileService fileService,
        ITranscodingService transcodingService,
        UserManager<AppUser> userManager
    )
    {
        var otherArtists = request.OtherArtists.Distinct().ToArray();

        var user = await userManager.GetCurrentUser(claims);

        // Validate
        var artist = await dbContext
            .Users.GetArtist(user.Id)
            .FirstOrDefaultAsync();

        if (artist is null)
        {
            return TypedResults.BadRequest("User is not an artist");
        }

        var album = await dbContext
            .Albums.Where(a => a.Id == request.AlbumId)
            .FirstOrDefaultAsync();

        if (album is null)
        {
            return TypedResults.BadRequest("Album not found");
        }

        {
            var count = await dbContext
                .ArtistAlbums.Where(e =>
                    e.AlbumId == request.AlbumId
                    && otherArtists.Contains(e.ArtistId)
                )
                .CountAsync();

            if (count != otherArtists.Length)
            {
                return TypedResults.BadRequest("One or more artists not found");
            }
        }

        // Create

        var track = new Track
        {
            Title = request.Title,
            Genre = request.Genre,
            AlbumId = request.AlbumId,
            DataUri = null,
            Duration = 0,
            PrimaryArtistId = artist.Id,
        };

        dbContext.Tracks.Add(track);

        foreach (var otherArtist in otherArtists)
        {
            if (artist.Id != otherArtist)
            {
                dbContext.TrackArtists.Add(
                    new() { TrackId = track.Id, ArtistId = otherArtist }
                );
            }
        }

        var file = await fileService.SaveFile(request.File);

        dbContext.Files.Add(file);

        await dbContext.SaveChangesAsync();

        // Enqueue transcoding task
        var transcodeId = await transcodingService.Enqueue(track.Id, file.Id);

        var response = new Response(
            Id: track.Id,
            Title: track.Title,
            Genre: track.Genre,
            CreationDate: track.CreationDate,
            AlbumId: track.AlbumId,
            PrimaryArtistId: track.PrimaryArtistId,
            OtherArtistIds: otherArtists,
            TranscodingId: transcodeId
        );

        return TypedResults.Ok(response);
    }
}