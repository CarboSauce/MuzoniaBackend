using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Services;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Playlists;

public class EditPlaylistInfo : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPatch("/{id}", Handle);

    public record Request(
        string? Name,
        string? Description,
        string? IsPublic,
        IFormFile? File
    );

    public record Response(
        EntityId Id,
        string Name,
        string Description,
        bool IsPublic,
        Uri? ImageUri
    );

    private static async Task<Results<Ok<Response>, BadRequest>> Handle(
        EntityId id,
        [FromForm] Request req,
        ApiDbContext dbContext,
        UserService userService,
        IFileWriter fileWriter
    )
    {
        var user = await userService.GetUser();

        var playlist = await dbContext
            .Playlists.Where(e => e.Id == id)
            .FirstOrDefaultAsync();

        if (
            playlist is null
            || playlist.UserId != user.Id
            || await userService.IsNotAdmin(user)
        )
        {
            return TypedResults.BadRequest();
        }

        if (req.Name is not null)
        {
            playlist.Name = req.Name;
        }

        if (req.Description is not null)
        {
            playlist.Description = req.Description;
        }

        if (req.IsPublic is not null)
        {
            playlist.IsPublic = bool.Parse(req.IsPublic);
        }

        if (req.File is not null)
        {
            var imageUri = await fileWriter.WriteImage(req.File);
            playlist.ImageUri = imageUri;
        }

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
