using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Playlists;

public class ReorderEntry : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/{playlistId}/reorder", Handle);

    public record Request(EntityId entryId, int oldIndex, int newIndex);

    private static async Task<Results<Ok, BadRequest>> Handle(
        EntityId playlistId,
        [FromBody] Request req,
        ApiDbContext dbContext,
        UserService userService
    )
    {
        var (entryId, oldIndex, newIndex) = req;
        if (oldIndex == newIndex)
            return TypedResults.BadRequest();

        var (user, isAdmin) = await userService.CurrentUser();

        var playlist = await dbContext.Playlists.FindAsync(playlistId);
        if (playlist is null || (playlist.UserId != user.Id && !isAdmin))
        {
            return TypedResults.BadRequest();
        }

        var hasEntry = await dbContext.PlaylistTracks.AnyAsync(pt =>
            pt.PlaylistId == playlistId && pt.Id == entryId
        );
        if (!hasEntry)
        {
            return TypedResults.BadRequest();
        }

        await using var tScope =
            await dbContext.Database.BeginTransactionAsync();
        try
        {
            await ExecuteReordering(
                playlistId,
                entryId,
                dbContext,
                newIndex,
                oldIndex
            );

            await tScope.CommitAsync();
        }
        catch
        {
            await tScope.RollbackAsync();
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok();
    }

    private static async Task ExecuteReordering(
        EntityId playlistId,
        EntityId entryId,
        ApiDbContext dbContext,
        int newIndex,
        int oldIndex
    )
    {
        var isMovingDown = newIndex > oldIndex;
        if (isMovingDown)
        {
            await dbContext
                .PlaylistTracks.Where(pt =>
                    pt.PlaylistId == playlistId
                    && pt.Index > oldIndex
                    && pt.Index <= newIndex
                )
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(pt => pt.Index, pt => pt.Index - 1)
                );
        }
        else
        {
            await dbContext
                .PlaylistTracks.Where(pt =>
                    pt.PlaylistId == playlistId
                    && pt.Index >= newIndex
                    && pt.Index < oldIndex
                )
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(pt => pt.Index, pt => pt.Index + 1)
                );
        }
        await dbContext
            .PlaylistTracks.Where(pt =>
                pt.PlaylistId == playlistId && pt.Id == entryId
            )
            .ExecuteUpdateAsync(s =>
                s.SetProperty(pt => pt.Index, pt => newIndex)
            );
    }
}
