using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Muzonia.Api.Common;
using Muzonia.Core.Services;
using Muzonia.Core.Services.Api;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.Playlists;

public class CreatePlaylist : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", Handle);

    public class Request
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsPublic { get; set; }
        public IFormFile? File { get; set; }
    }

    public record Response(
        EntityId Id,
        string Name,
        string Description,
        bool IsPublic,
        Uri? ImageUri
    );

    private static async Task<Results<Ok<Response>, BadRequest>> Handle(
        [FromForm] Request req,
        ApiDbContext dbContext,
        IFileWriter fileWriter,
        HttpContext context
    )
    {
        var fileUri = req.File is not null
            ? await fileWriter.WriteAsync(
                req.File,
                "images/",
                EntityId.NewGuid().ToString()
            )
            : null;

        var userId = context.GetUserId();
        var playlist = new Playlist
        {
            Description = req.Description,
            Name = req.Name,
            ImageUri = fileUri,
            UserId = userId,
            IsPublic = req.IsPublic,
            TrackCount = 0,
        };

        dbContext.Playlists.Add(playlist);

        await dbContext.SaveChangesAsync();

        return TypedResults.Ok(
            new Response(
                playlist.Id,
                playlist.Name,
                playlist.Description,
                playlist.IsPublic,
                playlist.ImageUri
            )
        );
    }
}
