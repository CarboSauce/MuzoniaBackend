using System.Security.Claims;
using GreenDonut.Data;
using HotChocolate.Execution;
using Muzonia.Api.Common;
using Muzonia.Api.GraphQL.Tracks;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Playbacks;

[ObjectType<PlaybackQueue>]
public static partial class PlaybackType
{
    [UsePaging]
    [UseSorting]
    [BindMember(nameof(PlaybackQueue.Entries))]
    public static async Task<Page<QueueEntry>> GetEntriesAsync(
        [Parent(requires: nameof(PlaybackQueue.Id))]
            PlaybackQueue playbackQueue,
        IQueueEntriesByPlaybackIdDataLoader dataLoader,
        PagingArguments paging,
        QueryContext<QueueEntry> query,
        ClaimsPrincipal claims,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;

        return await dataLoader
            .SetState("userId", userId)
            .With(paging, query)
            .LoadRequiredAsync(playbackQueue.Id, ct);
    }
}

[ObjectType<QueueEntry>]
public static partial class QueueEntryType
{
    [BindMember(nameof(QueueEntry.Track))]
    public static async Task<Track> GetTrackAsync(
        [Parent(requires: nameof(QueueEntry.Id))] QueueEntry queueEntry,
        ITrackByQueueEntryIdDataLoader dataLoader,
        ISelection selector,
        CancellationToken ct
    )
    {
        return await dataLoader
            .Select(selector)
            .LoadRequiredAsync(queueEntry.Id, ct);
    }
}
