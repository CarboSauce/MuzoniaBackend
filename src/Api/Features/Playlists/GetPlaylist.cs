using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Playlists;

public class GetPlaylist : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id}", Handle);

    public record Response(
        string Title,
        string Description,
        EntityId Id,
        EntityId UserId,
        DateTime CreationDate,
        bool IsPublic,
        Uri? ImageUri
    );

    private static async Task<Results<Ok<Response>, BadRequest>> Handle(
        EntityId id,
        ApiDbContext dbContext,
        UserService userService
    )
    {
        var user = await userService.GetUser();

        var playlist = await dbContext
            .Playlists.Where(e =>
                e.Id == id //&& (e.UserId == userId || e.IsPublic)
            )
            .Select(e => new Response(
                e.Name,
                e.Description,
                e.Id,
                e.UserId,
                e.CreationDate,
                e.IsPublic,
                e.ImageUri
            ))
            .FirstOrDefaultAsync();

        if (
            playlist is null
            || playlist.UserId != user.Id
            || await userService.IsNotAdmin(user)
        )
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(
            new Response(
                playlist.Title,
                playlist.Description,
                playlist.Id,
                playlist.UserId,
                playlist.CreationDate,
                playlist.IsPublic,
                playlist.ImageUri
            )
        );
    }
}
