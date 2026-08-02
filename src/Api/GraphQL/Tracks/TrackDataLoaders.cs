using GreenDonut.Data;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.GraphQL.Artists;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Tracks;

public static class TrackDataLoaders
{
    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Track>
    > TrackByIdAsync(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        ISelectorBuilder selector,
        CancellationToken ct
    )
    {
        return await dbContext
            .Tracks.Where(t => ids.Contains(t.Id))
            .OrderBy(t => t.Id)
            .Select(t => t.Id, selector)
            .ToDictionaryAsync(t => t.Id, ct);
    }

    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Page<Track>>
    > TracksByArtistIdAsync(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        PagingArguments pagingArguments,
        QueryContext<Track> queryContext,
        CancellationToken ct
    )
    {
        var rows = await dbContext
            .TrackArtists.AsNoTracking()
            .Where(ta => ids.Contains(ta.ArtistId))
            .Select(ta => ta.Track)
            .OrderBy(t => t.Id)
            .With(queryContext)
            .ToBatchPageAsync(
                t => t.Artists.First(a => ids.Contains(a.Id)).Id,
                pagingArguments,
                ct
            );
        return rows;
    }

    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Page<Track>>
    > TracksByAlbumIdAsync(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        PagingArguments pagingArguments,
        QueryContext<Track> query,
        ISelectorBuilder selector,
        CancellationToken ct
    )
    {
        return await dbContext
            .Albums.Where(a => ids.Contains(a.Id))
            .Select(a => a.Id, selector)
            .SelectMany(a => a.Tracks ?? Array.Empty<Track>())
            .With(query)
            .ToBatchPageAsync(
                t => t.AlbumId!.Value,
                t => t,
                pagingArguments,
                ct
            );
    }

    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Track>
    > TrackByQueueEntryId(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        ISelectorBuilder selector,
        CancellationToken ct
    )
    {
        return await dbContext
            .QueueEntries.AsNoTracking()
            .Where(q => ids.Contains(q.QueueId))
            .Select(q => new { q.Id, q.Track })
            .Select(q => q.Id, selector)
            .ToDictionaryAsync(t => t.Id, t => t.Track, ct);
    }
}
