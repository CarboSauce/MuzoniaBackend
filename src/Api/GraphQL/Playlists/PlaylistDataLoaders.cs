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
        DataLoaderFetchContext<Playlist> fetchContext,
        PagingArguments pagingArguments,
        QueryContext<Playlist> query,
        CancellationToken ct
    )
    {
        var userId = fetchContext.GetRequiredState<EntityId>("userId");
        return await dbContext
            .Playlists.AsNoTracking()
            .Where(p => ids.Contains(p.UserId))
            .Where(p => p.IsPublic || p.UserId == userId)
            .With(query)
            .ToBatchPageAsync(p => p.UserId, pagingArguments, ct);
    }

    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Page<Playlist>>
    > PlaylistsByIdAsync(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        DataLoaderFetchContext<Playlist> fetchContext,
        PagingArguments pagingArguments,
        QueryContext<Playlist> query,
        CancellationToken ct
    )
    {
        var userId = fetchContext.GetRequiredState<EntityId>("userId");
        return await dbContext
            .Playlists.AsNoTracking()
            .Where(p =>
                ids.Contains(p.Id) && (p.IsPublic || p.UserId == userId)
            )
            .With(query)
            .ToBatchPageAsync(p => p.Id, pagingArguments, ct);
    }

    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Playlist>
    > PlaylistByIdAsync(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        DataLoaderFetchContext<Playlist> fetchContext,
        QueryContext<Playlist> query,
        CancellationToken ct
    )
    {
        var userId = fetchContext.GetRequiredState<EntityId>("userId");
        return await dbContext
            .Playlists.AsNoTracking()
            .Where(p =>
                ids.Contains(p.Id) && (p.IsPublic || p.UserId == userId)
            )
            .With(query)
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
            .ToBatchPageAsync(p => p.Id, pagingArguments, ct);
    }
}
