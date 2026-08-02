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
        DataLoaderFetchContext<QueueEntry> fetchContext,
        PagingArguments pagingArguments,
        QueryContext<QueueEntry> query,
        CancellationToken ct
    )
    {
        var userId = fetchContext.GetRequiredState<EntityId>("userId");
        return await dbContext
            .PlaybackQueues.AsNoTracking()
            .Where(p => playbackIds.Contains(p.Id))
            .Where(p => p.OwnerId == userId || p.IsPublic == true) //)
            .SelectMany(p => p.Entries)
            .With(query)
            .ToBatchPageAsync(p => p.QueueId, pagingArguments, ct);
    }
}
