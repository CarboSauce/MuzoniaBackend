using GreenDonut.Data;
using Microsoft.EntityFrameworkCore;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Playbacks;

public static class PlaybackDataLoaders
{
    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Page<QueueEntry>>
    > QueueEntriesByPlaybackIdAsync(
        IReadOnlyList<EntityId> playbackIds,
        ApiDbContext dbContext,
        PagingArguments pagingArguments,
        [DataLoaderState("userId")] EntityId userId,
        QueryContext<QueueEntry> query,
        CancellationToken ct
    )
    {
        return await dbContext
            .PlaybackQueues.AsNoTracking()
            .Where(p => playbackIds.Contains(p.Id))
            .Where(p => p.OwnerId == userId || p.IsPublic == true)
            .SelectMany(p => p.Entries)
            .With(query)
            .OrderBy(p => p.Id)
            .ToBatchPageAsync(p => p.QueueId, pagingArguments, ct);
    }
}
