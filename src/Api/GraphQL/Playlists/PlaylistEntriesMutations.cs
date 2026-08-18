using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Playlists;

[MutationType]
public static partial class PlaylistEntriesMutations
{
    public static async Task<PlaylistTrack> AddPlaylistEntryAsync(
        AddPlaylistEntryInput input,
        ApiDbContext dbContext,
        CancellationToken ct
    )
    {
        var playlist = await dbContext.Playlists.FirstOrDefaultAsync(
            e => e.Id == input.PlaylistId,
            ct
        );

        if (playlist is null)
        {
            throw new GraphQLException("Playlist not found");
        }

        var track = await dbContext.Tracks.FirstOrDefaultAsync(
            e => e.Id == input.TrackId && e.DataUri != null,
            ct
        );

        if (track is null)
        {
            throw new GraphQLException("Track not found");
        }

        var entry = new PlaylistTrack
        {
            PlaylistId = input.PlaylistId,
            TrackId = input.TrackId,
            Index = playlist.TrackCount,
        };

        dbContext.PlaylistTracks.Add(entry);
        playlist.TrackCount += 1;

        await dbContext.SaveChangesAsync(ct);

        return entry;
    }

    public static async Task<EntityId> RemovePlaylistEntryAsync(
        [ID<Playlist>] EntityId playlistId,
        [ID<PlaylistTrack>] EntityId entryId,
        ApiDbContext dbContext,
        ClaimsPrincipal claims,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;
        var playlist = await dbContext
            .Playlists.Where(e => e.Id == playlistId)
            .FirstOrDefaultAsync(ct);

        if (playlist is null || (playlist.UserId != userId))
        {
            throw new GraphQLException("Playlist not found");
        }
        var track = await dbContext
            .PlaylistTracks.Where(e =>
                e.PlaylistId == playlistId && e.Id == entryId
            )
            .FirstOrDefaultAsync(ct);

        if (track is null)
            throw new GraphQLException("Track not found");

        await dbContext.UseTransactionAsync(async () =>
        {
            await dbContext
                .PlaylistTracks.Where(pt =>
                    pt.PlaylistId == playlistId && pt.Index > track.Index
                )
                .ExecuteUpdateAsync(
                    s => s.SetProperty(pt => pt.Index, pt => pt.Index - 1),
                    ct
                );

            await dbContext
                .PlaylistTracks.Where(pt => pt.Id == entryId)
                .ExecuteDeleteAsync(ct);
        });

        return entryId;
    }

    public static async Task<EntityId> ReorderPlaylistEntryAsync(
        [ID<Playlist>] EntityId playlistId,
        [ID<PlaylistTrack>] EntityId entryId,
        int oldIndex,
        int newIndex,
        ApiDbContext dbContext,
        ClaimsPrincipal claims,
        CancellationToken ct
    )
    {
        if (oldIndex == newIndex)
            throw new GraphQLException(
                "oldIndex and newIndex cannot be the same"
            );

        var userId = claims.UserId;

        var playlist = await dbContext.Playlists.FindAsync(playlistId, ct);
        if (playlist is null || playlist.UserId != userId)
        {
            throw new GraphQLException("Playlist not found");
        }

        var hasEntry = await dbContext.PlaylistTracks.AnyAsync(
            pt => pt.PlaylistId == playlistId && pt.Id == entryId,
            ct
        );
        if (!hasEntry)
        {
            throw new GraphQLException("Entry not found");
        }

        await dbContext.UseTransactionAsync(async () =>
        {
            await ExecuteReordering(
                playlistId,
                entryId,
                dbContext,
                newIndex,
                oldIndex
            );
        });

        return entryId;
        async Task ExecuteReordering(
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
                    .ExecuteUpdateAsync(
                        s => s.SetProperty(pt => pt.Index, pt => pt.Index - 1),
                        ct
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
                    .ExecuteUpdateAsync(
                        s => s.SetProperty(pt => pt.Index, pt => pt.Index + 1),
                        ct
                    );
            }
            await dbContext
                .PlaylistTracks.Where(pt =>
                    pt.PlaylistId == playlistId && pt.Id == entryId
                )
                .ExecuteUpdateAsync(
                    s => s.SetProperty(pt => pt.Index, pt => newIndex),
                    ct
                );
        }
    }
}

public record AddPlaylistEntryInput(
    [property: ID<Playlist>] EntityId PlaylistId,
    [property: ID<Track>] EntityId TrackId
);
