using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Hubs;

[Authorize]
public class PlayerHub(ILogger<PlayerHub> logger, ApiDbContext dbContext) : Hub
{
    private EntityId UserId => new EntityId(Context.UserIdentifier!);
    private string UserIdentifier => Context.UserIdentifier!;
    private string ConnectionId => Context.ConnectionId;

    private static readonly ConnectionMapping<string> UserMapping = new();

    public override async Task OnConnectedAsync()
    {
        var clientId = Context.ConnectionId;
        var userId = UserIdentifier;

        logger.LogInformation(
            "Client {clientId} connected as user({userId})",
            Context.ConnectionId,
            userId
        );

        UserMapping.Add(userId, clientId);

        var queueId = await dbContext
            .CurrentQueues.Where(c => c.UserId == UserId)
            .Select(c => c.QueueId)
            .FirstOrDefaultAsync();

        await Groups.AddToGroupAsync(clientId, queueId.ToString());
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Client {clientId} disconnected", ConnectionId);

        UserMapping.Remove(UserIdentifier, ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SetIsPlaying(bool isPlaying, int timestamp)
    {
        var userId = UserId;

        var queueId = await EnsureUserCanModifyQueue(userId);

        await dbContext
            .PlaybackQueues.Where(p => p.Id == queueId)
            .ExecuteUpdateAsync(p =>
                p.SetProperty(q => q.IsPlaying, isPlaying)
                    .SetProperty(q => q.Timestamp, timestamp)
            );

        await SendToGroupExcept(
            queueId,
            ConnectionId,
            "IsPlaying",
            new { isPlaying, timestamp }
        );
    }

    public async Task Seek(int position)
    {
        var userId = UserId;

        if (position < 0)
        {
            throw new HubException(
                "Position must be greater than or equal to 0"
            );
        }

        var queueId = await EnsureUserCanModifyQueue(userId);

        await dbContext
            .PlaybackQueues.Where(p => p.Id == queueId)
            .ExecuteUpdateAsync(p => p.SetProperty(q => q.Timestamp, position));

        await SendToGroupExcept(
            queueId,
            ConnectionId,
            "Seek",
            new { position }
        );
    }

    public async Task Sync(int timestamp)
    {
        var userId = UserId;

        var queueId = await EnsureUserCanModifyQueue(userId);

        await dbContext
            .PlaybackQueues.Where(p => p.Id == queueId)
            .ExecuteUpdateAsync(p =>
                p.SetProperty(q => q.Timestamp, timestamp)
            );

        var conIds = UserMapping.GetAll(userId.ToString());

        await Clients
            .GroupExcept(queueId.ToString(), conIds)
            .SendAsync("Sync", new { timestamp });
    }

    public async Task SetIsRepeat(bool isRepeat)
    {
        var userId = UserId;

        var queueId = await EnsureUserCanModifyQueue(userId);

        await dbContext
            .PlaybackQueues.Where(p => p.Id == queueId)
            .ExecuteUpdateAsync(p => p.SetProperty(q => q.IsRepeat, isRepeat));

        await SendToGroupExcept(
            queueId,
            ConnectionId,
            "IsRepeat",
            new { isRepeat }
        );
    }

    public async Task SetIsRandom(bool isRandom)
    {
        var userId = UserId;

        var queueId = await EnsureUserCanModifyQueue(userId);
        await dbContext
            .PlaybackQueues.Where(p => p.Id == queueId)
            .ExecuteUpdateAsync(p => p.SetProperty(q => q.IsRandom, isRandom));

        await SendToGroupExcept(
            queueId,
            ConnectionId,
            "IsRandom",
            new { isRandom }
        );
    }

    public async Task AddEntry(EntityId trackId)
    {
        var userId = UserId;

        var queueId = await EnsureUserCanModifyQueue(userId);

        var queue = await dbContext
            .PlaybackQueues.Where(p => p.Id == queueId)
            .FirstOrDefaultAsync();

        if (queue is null)
        {
            throw new HubException("User does not have a playback queue");
        }

        var entry = new QueueEntry
        {
            QueueId = queue.Id,
            TrackId = trackId,
            Index = queue.TrackCount,
        };

        dbContext.QueueEntries.Add(entry);

        queue.TrackCount += 1;

        await dbContext.SaveChangesAsync();

        await SendToGroupExcept(
            queueId,
            ConnectionId,
            "EntryAdded",
            new
            {
                entry.Id,
                entry.TrackId,
                entry.Index,
                entry.QueueId
            }
        );
    }

    public async Task RemoveEntry(EntityId entryId)
    {
        var userId = UserId;

        var queueId = await EnsureUserCanModifyQueue(userId);

        var entry = await dbContext
            .QueueEntries.Where(e => e.Id == entryId)
            .FirstOrDefaultAsync();

        if (entry is null)
        {
            throw new HubException("Entry does not exist");
        }

        var playbackQueue = await dbContext
            .PlaybackQueues.Where(p => p.Id == queueId)
            .FirstOrDefaultAsync();

        if (playbackQueue is null)
        {
            throw new HubException("Queue does not exist");
        }

        await dbContext.UseTransactionAsync(async () =>
        {
            await dbContext
                .QueueEntries.Where(e =>
                    e.QueueId == queueId && e.Index > entry.Index
                )
                .ExecuteUpdateAsync(e =>
                    e.SetProperty(q => q.Index, q => q.Index - 1)
                );

            if (playbackQueue.TrackCount == entry.Index + 1)
            {
                playbackQueue.CurrentIndex = 0;
            }
            playbackQueue.TrackCount -= 1;

            dbContext.QueueEntries.Remove(entry);

            await dbContext.SaveChangesAsync();
        });

        await SendToGroupExcept(
            queueId,
            ConnectionId,
            "EntryRemoved",
            new { entryId }
        );
    }

    public async Task PermuteQueue(int[] newIndices)
    {
        var userId = UserId;

        var queueId = await EnsureUserCanModifyQueue(userId);

        var entries = await dbContext
            .QueueEntries.Where(e => e.QueueId == queueId)
            .OrderBy(e => e.Id)
            .ToListAsync();

        if (entries.Count != newIndices.Length)
        {
            throw new HubException("Invalid permutation length");
        }

        if (!newIndices.All(i => i >= 0 && i < entries.Count))
        {
            throw new HubException("Invalid permutation indexes");
        }

        for (var i = 0; i < entries.Count; i++)
        {
            entries[i].Index = newIndices[i];
        }

        await dbContext.SaveChangesAsync();

        await SendToGroupExcept(
            queueId,
            ConnectionId,
            "QueuePermuted",
            entries.Select(e => new { e.Id, e.Index })
        );
    }

    public async Task CleanPlay(EntityId[] trackIds)
    {
        var userId = UserId;

        var queue = await EnsureUserCanModifyQueueAndGet(userId);

        var entries = await dbContext
            .QueueEntries.Where(e => e.QueueId == queue.Id)
            .ToListAsync();

        dbContext.QueueEntries.RemoveRange(entries);
        queue.CurrentIndex = 0;
        queue.TrackCount = 0;

        foreach (var trackId in trackIds)
        {
            var entry = new QueueEntry
            {
                QueueId = queue.Id,
                TrackId = trackId,
                Index = queue.TrackCount,
            };
            queue.TrackCount += 1;

            dbContext.QueueEntries.Add(entry);
        }

        await dbContext.SaveChangesAsync();

        await SendToGroupExcept(
            queue.Id,
            ConnectionId,
            "CleanPlay",
            new { trackIds }
        );
    }

    public async Task SetCurrentTrack(EntityId entryId, int timestamp)
    {
        var userId = UserId;

        var queueId = await EnsureUserCanModifyQueue(userId);

        var entry = await dbContext
            .QueueEntries.Where(e => e.Id == entryId && e.QueueId == queueId)
            .FirstOrDefaultAsync();

        if (entry is null)
        {
            throw new HubException("Entry does not exist");
        }

        await dbContext
            .PlaybackQueues.Where(p => p.Id == queueId)
            .ExecuteUpdateAsync(p =>
                p.SetProperty(q => q.CurrentIndex, entry.Index)
                    .SetProperty(q => q.Timestamp, timestamp)
            );

        await SendToGroupExcept(
            queueId,
            ConnectionId,
            "CurrentTrack",
            new { entryId, timestamp }
        );
    }

    public async Task ReorderEntry(int oldIndex, int newIndex)
    {
        var userId = UserId;

        var queueId = await EnsureUserCanModifyQueue(userId);

        var entry = await dbContext
            .QueueEntries.Where(e =>
                e.Index == oldIndex && e.QueueId == queueId
            )
            .FirstOrDefaultAsync();

        if (entry is null)
        {
            throw new HubException("Entry does not exist");
        }

        await dbContext.UseTransactionAsync(async () =>
        {
            var isMovingDown = newIndex > oldIndex;

            if (isMovingDown)
            {
                await dbContext
                    .QueueEntries.Where(e =>
                        e.QueueId == entry.QueueId
                        && e.Index > oldIndex
                        && e.Index <= newIndex
                    )
                    .ExecuteUpdateAsync(e =>
                        e.SetProperty(q => q.Index, q => q.Index - 1)
                    );
            }
            else
            {
                await dbContext
                    .QueueEntries.Where(e =>
                        e.QueueId == entry.QueueId
                        && e.Index < oldIndex
                        && e.Index >= newIndex
                    )
                    .ExecuteUpdateAsync(e =>
                        e.SetProperty(q => q.Index, q => q.Index + 1)
                    );
            }

            entry.Index = newIndex;
            await dbContext.SaveChangesAsync();
        });

        await SendToGroupExcept(
            queueId,
            ConnectionId,
            "EntryReordered",
            new { entryId = entry.Id, newIndex }
        );
    }

    public async Task JoinQueue(EntityId queueId)
    {
        var userId = UserId;

        var queue = await dbContext
            .PlaybackQueues.Where(p => p.Id == queueId && p.IsPublic)
            .FirstOrDefaultAsync();

        if (queue is null)
        {
            throw new HubException("Queue does not exist or is not public");
        }

        var userQueue = await dbContext
            .QueueUsers.Where(u => u.QueueId == queueId && u.UserId == userId)
            .FirstOrDefaultAsync();

        if (userQueue is null)
        {
            await dbContext.UseTransactionAsync(async () =>
            {
                dbContext.QueueUsers.Add(
                    new QueueUser
                    {
                        QueueId = queueId,
                        UserId = userId,
                        IsBanned = false,
                    }
                );

                await dbContext
                    .CurrentQueues.Where(c => c.UserId == userId)
                    .ExecuteUpdateAsync(c =>
                        c.SetProperty(q => q.QueueId, queueId)
                    );

                await dbContext.SaveChangesAsync();
            });
        }
        else if (userQueue.IsBanned)
        {
            throw new HubException("User is banned from the queue");
        }
        else
        {
            throw new HubException("User is already in the queue");
        }

        var conIds = UserMapping.GetAll(userId.ToString());
        var oldQueueId = await GetGroupId(userId);

        foreach (var conId in conIds)
        {
            await Groups.RemoveFromGroupAsync(conId, oldQueueId.ToString());
            await Groups.AddToGroupAsync(conId, queueId.ToString());
        }

        await SendToGroupExcept(
            queueId,
            ConnectionId,
            "UserJoined",
            new { userId }
        );
    }

    public async Task LeaveQueue(EntityId queueId)
    {
        var userId = UserId;

        var userQueue = await dbContext
            .QueueUsers.Where(u =>
                u.QueueId == queueId
                && u.UserId == userId
                && u.Queue.OwnerId != userId
            )
            .FirstOrDefaultAsync();

        if (userQueue is null)
        {
            throw new HubException("User is not in the queue");
        }

        var mainQueueId = await GetOwnedQueueId(userId);
        await dbContext.UseTransactionAsync(async () =>
        {
            dbContext.QueueUsers.Remove(userQueue);

            await dbContext
                .CurrentQueues.Where(c => c.UserId == userId)
                .ExecuteUpdateAsync(c =>
                    c.SetProperty(q => q.QueueId, mainQueueId)
                );

            await dbContext.SaveChangesAsync();
        });

        var conIds = UserMapping.GetAll(userId.ToString());

        await SendToGroupExcept(
            queueId,
            ConnectionId,
            "UserLeft",
            new { userId }
        );

        foreach (var conId in conIds)
        {
            await Groups.RemoveFromGroupAsync(conId, queueId.ToString());
        }
    }

    public async Task KickUser(EntityId queueId, EntityId userId)
    {
        var kickerId = UserId;

        var _ = await EnsureIsOwner(queueId, kickerId);

        var user = await dbContext
            .QueueUsers.Where(u =>
                u.QueueId == queueId
                && u.UserId == userId
                && u.Queue.OwnerId != userId
            )
            .FirstOrDefaultAsync();

        if (user is null)
        {
            throw new HubException("User is not in the queue");
        }

        var ownedQueueId = await GetOwnedQueueId(userId);

        await dbContext.UseTransactionAsync(async () =>
        {
            await dbContext
                .CurrentQueues.Where(c => c.UserId == userId)
                .ExecuteUpdateAsync(c =>
                    c.SetProperty(q => q.QueueId, ownedQueueId)
                );

            user.IsBanned = true;
            await dbContext.SaveChangesAsync();
        });

        var conIds = UserMapping.GetAll(userId.ToString());

        await SendToGroupExcept(
            queueId,
            ConnectionId,
            "UserKicked",
            new { userId }
        );

        foreach (var conId in conIds)
        {
            await Groups.RemoveFromGroupAsync(conId, queueId.ToString());
        }
    }

    public async Task OpenQueue()
    {
        var userId = UserId;

        var queue = await dbContext
            .PlaybackQueues.Where(e => e.OwnerId == userId)
            .FirstOrDefaultAsync();

        if (queue is null)
        {
            throw new HubException("User does not have a playback queue");
        }

        queue.IsPublic = true;
        queue.IsModifiable = true;
        await dbContext.SaveChangesAsync();
    }

    public async Task CloseQueue()
    {
        var userId = UserId;

        var queue = await dbContext
            .PlaybackQueues.Where(e => e.OwnerId == userId)
            .FirstOrDefaultAsync();

        if (queue is null)
        {
            throw new HubException("User does not have a playback queue");
        }

        var userQueues = await dbContext
            .PlaybackQueues.Where(e =>
                dbContext.QueueUsers.Any(qc => qc.UserId == e.OwnerId)
            )
            .Select(e => new { queueId = e.Id, userId = e.OwnerId })
            .ToArrayAsync();

        await dbContext.UseTransactionAsync(async () =>
        {
            foreach (var userQueue in userQueues)
            {
                await dbContext
                    .CurrentQueues.Where(c => c.UserId == userQueue.userId)
                    .ExecuteUpdateAsync(c =>
                        c.SetProperty(q => q.QueueId, userQueue.queueId)
                    );
                if (queue.OwnerId != userQueue.userId)
                    await dbContext
                        .QueueUsers.Where(u =>
                            u.UserId == userQueue.userId
                            && u.QueueId == queue.Id
                        )
                        .ExecuteDeleteAsync();
            }

            queue.IsPublic = false;
            queue.IsModifiable = false;
            await dbContext.SaveChangesAsync();
        });

        await SendToGroupExcept(
            queue.Id,
            ConnectionId,
            "QueueClosed",
            new { queueId = queue.Id }
        );
    }

    private IClientProxy QueueGroupExcept(string groupName, string exceptId) =>
        Clients.GroupExcept(groupName, exceptId);

    private async Task<EntityId> GetGroupId(EntityId userId)
    {
        var res = await dbContext
            .CurrentQueues.Where(c => c.UserId == userId)
            .Select(p => p.QueueId)
            .FirstOrDefaultAsync();

        return res;
    }

    private async Task<EntityId> GetOwnedQueueId(EntityId userId)
    {
        var res = await dbContext
            .PlaybackQueues.Where(p => p.OwnerId == userId)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();

        return res;
    }

    private async Task SendToGroupExcept<T>(
        EntityId queueId,
        string exceptId,
        string message,
        T value
    )
    {
        await QueueGroupExcept(queueId.ToString(), exceptId)
            .SendAsync(message, value);
    }

    private async Task SendToGroup<T>(EntityId queueId, string message, T value)
    {
        // var groupName = await GetGroupId(UserId);

        await Clients.Group(queueId.ToString()).SendAsync(message, value);
    }

    private async Task<EntityId> EnsureUserCanModifyQueue(EntityId userId)
    {
        var queueId = await GetGroupId(userId);

        var queue = await dbContext
            .PlaybackQueues.Where(p => p.Id == queueId)
            .Where(p =>
                p.OwnerId == userId
                || (p.IsModifiable && p.QueueUsers.Any(u => u.UserId == userId))
            )
            .AnyAsync();

        if (!queue)
        {
            throw new HubException("User does not have a playback queue");
        }

        return queueId;
    }

    private async Task<PlaybackQueue> EnsureUserCanModifyQueueAndGet(
        EntityId userId
    )
    {
        var queueId = await GetGroupId(userId);

        var queue = await dbContext
            .PlaybackQueues.Where(p => p.Id == queueId)
            .Where(p =>
                p.OwnerId == userId
                || (p.IsModifiable && p.QueueUsers.Any(u => u.UserId == userId))
            )
            .FirstOrDefaultAsync();

        if (queue is null)
        {
            throw new HubException("User does not have a playback queue");
        }

        return queue;
    }

    private async Task<PlaybackQueue> EnsureIsOwner(
        EntityId queueId,
        EntityId userId
    )
    {
        var queue = await dbContext
            .PlaybackQueues.Where(p => p.Id == queueId && p.OwnerId == userId)
            .FirstOrDefaultAsync();

        if (queue is null)
        {
            throw new HubException("User is not the owner of the queue");
        }

        return queue;
    }

    private async Task<EntityId> EnsureUserCanAccessQueue(EntityId userId)
    {
        var queueId = await GetGroupId(userId);

        var queue = await dbContext
            .PlaybackQueues.Where(p => p.Id == queueId)
            .Where(p =>
                p.OwnerId == userId || p.QueueUsers.Any(u => u.UserId == userId)
            )
            .AnyAsync();

        if (!queue)
        {
            throw new HubException("User does not have a playback queue");
        }

        return queueId;
    }
}
