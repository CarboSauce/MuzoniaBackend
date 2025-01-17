using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Playlists;

public class RemoveEntry : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/{id}/entry/{entryId}", Handle);

    public record Request();

    public record Response();

    private static async Task<Results<NoContent, BadRequest>> Handle(
        EntityId id,
        EntityId entryId,
        ApiDbContext dbContext,
        UserService userService
    )
    {
        var (user, isAdmin) = await userService.CurrentUser();

        var playlist = await dbContext
            .Playlists.Where(e => e.Id == id)
            .FirstOrDefaultAsync();

        if (playlist is null || (playlist.UserId != user.Id && !isAdmin))
        {
            return TypedResults.BadRequest();
        }

        await dbContext
            .PlaylistTracks.Where(e => e.PlaylistId == id && e.Id == entryId)
            .ExecuteDeleteAsync();

        return TypedResults.NoContent();
    }
}
