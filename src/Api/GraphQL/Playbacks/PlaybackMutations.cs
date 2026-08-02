using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Playbacks;

[MutationType]
public static class PlaybackMutations
{
    static async Task<PlaybackQueue> UpdateQueueStateAsync(
        UpdateQueueStateInput input,
        ApiDbContext dbContext,
        ClaimsPrincipal claims,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;

        var queue = await dbContext
            .PlaybackQueues.Where(q => q.Id == input.QueueId)
            .SingleAsync(ct);

        if (queue.OwnerId != userId)
            throw new GraphQLException("User can't modify this queue");

        queue.CurrentIndex = input.Index;
        queue.IsPlaying = input.IsPlaying;
        queue.IsRepeat = input.IsRepeat;
        queue.Timestamp = input.Position;
        queue.IsRandom = input.IsRandom;

        dbContext.Update(queue);
        await dbContext.SaveChangesAsync(ct);

        return queue;
    }

    static async Task<QueueEntry> AddQueueEntryAsync(
        AddQueueEntryInput input,
        ApiDbContext dbContext,
        ClaimsPrincipal claims,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;

        var queue = await dbContext
            .PlaybackQueues.Where(q => q.Id == input.QueueId)
            .SingleAsync(ct);

        if (queue.OwnerId != userId)
            throw new GraphQLException("User can't modify this queue");

        var entry = new QueueEntry
        {
            QueueId = queue.Id,
            TrackId = input.TrackId,
            Index = queue.TrackCount,
        };

        dbContext.QueueEntries.Add(entry);

        queue.TrackCount += 1;

        await dbContext.SaveChangesAsync(ct);

        return entry;
    }
}

public record AddQueueEntryInput(EntityId QueueId, EntityId TrackId);

public record UpdateQueueStateInput(
    EntityId QueueId,
    int Index,
    bool IsPlaying,
    int Position,
    bool IsRepeat,
    bool IsRandom
);
