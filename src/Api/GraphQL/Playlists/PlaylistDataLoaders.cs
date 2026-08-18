using GreenDonut.Data;
using Microsoft.EntityFrameworkCore;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Playlists;

public static class PlaylistDataLoaders
{
    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Page<Playlist>>
    > PlaylistsByUserIdAsync(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        [DataLoaderState("userId")] EntityId userId,
        PagingArguments pagingArguments,
        QueryContext<Playlist> query,
        CancellationToken ct
    )
    {
        return await dbContext
            .Playlists.AsNoTracking()
            .Where(p => ids.Contains(p.UserId))
            .Where(p => p.IsPublic || p.UserId == userId)
            .With(query)
            .OrderBy(p => p.Id)
            .ToBatchPageAsync(p => p.UserId, pagingArguments, ct);
    }

    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Page<Playlist>>
    > PlaylistsByIdAsync(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        [DataLoaderState("userId")] EntityId userId,
        PagingArguments pagingArguments,
        QueryContext<Playlist> query,
        CancellationToken ct
    )
    {
        return await dbContext
            .Playlists.AsNoTracking()
            .Where(p =>
                ids.Contains(p.Id) && (p.IsPublic || p.UserId == userId)
            )
            .With(query)
            .OrderBy(p => p.Id)
            .ToBatchPageAsync(p => p.Id, pagingArguments, ct);
    }

    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Playlist>
    > PlaylistByIdAsync(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        [DataLoaderState("userId")] EntityId userId,
        QueryContext<Playlist> query,
        CancellationToken ct
    )
    {
        return await dbContext
            .Playlists.AsNoTracking()
            .Where(p =>
                ids.Contains(p.Id) && (p.IsPublic || p.UserId == userId)
            )
            .With(query)
            .OrderBy(p => p.Id)
            .ToDictionaryAsync(p => p.Id, ct);
    }

    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Page<PlaylistTrack>>
    > PlaylistTracksByPlaylistIdAsync(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        PagingArguments pagingArguments,
        QueryContext<PlaylistTrack> query,
        CancellationToken ct
    )
    {
        return await dbContext
            .PlaylistTracks.AsNoTracking()
            .Where(p => ids.Contains(p.PlaylistId))
            .With(query)
            .OrderBy(p => p.Id)
            .ToBatchPageAsync(p => p.PlaylistId, pagingArguments, ct);
    }

    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Page<PlaylistTrack>>
    > PlaylistTracksByIdAsync(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        PagingArguments pagingArguments,
        QueryContext<PlaylistTrack> query,
        CancellationToken ct
    )
    {
        return await dbContext
            .PlaylistTracks.AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .With(query)
            .OrderBy(p => p.Id)
            .ToBatchPageAsync(p => p.Id, pagingArguments, ct);
    }
}
