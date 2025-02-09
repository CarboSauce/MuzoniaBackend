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

    public override async Task OnConnectedAsync()
    {
        var clientId = Context.ConnectionId;
        var userId = UserIdentifier;

        logger.LogInformation(
            "Client {clientId} connected as user({userId})",
            Context.ConnectionId,
            userId
        );

        await Groups.AddToGroupAsync(Context.ConnectionId, userId);

        dbContext.PlaybackQueues.Add(
            new PlaybackQueue
            {
                Id = UserId,
                DeviceId = null,
                UserId = UserId,
                IsRepeat = false,
                Volume = 50,
                IsPlaying = false,
                IsRandom = false,
                CurrentIndex = 0,
                TrackCount = 0,
                Timestamp = 0,
            }
        );

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(
                "User({userId}) already has a playback queue",
                userId
            );
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var userId = UserId;

        var device = await dbContext
            .Devices.Where(d => d.Id == userId)
            .FirstOrDefaultAsync();

        if (device is not null)
        {
            logger.LogInformation(
                "User({userId}) with connection({connectionId}) disconnected from {deviceName}",
                Context.UserIdentifier,
                Context.ConnectionId,
                device.Name
            );

            await Clients
                .GroupExcept(UserIdentifier, connectionId)
                .SendAsync(
                    "DeviceLeft",
                    new { Id = device.Id, Name = device.Name, }
                );

            await Groups.RemoveFromGroupAsync(connectionId, UserIdentifier);

            dbContext.Remove(device);

            await dbContext.SaveChangesAsync();
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SetIsPlaying(bool isPlaying, int timestamp)
    {
        var userId = UserId;

        await dbContext
            .PlaybackQueues.Where(p => p.Id == userId)
            .ExecuteUpdateAsync(p =>
                p.SetProperty(q => q.IsPlaying, isPlaying)
            );

        await Clients
            .GroupExcept(UserIdentifier, ConnectionId)
            .SendAsync("IsPlaying", new { isPlaying, timestamp });
    }

    public async Task SetVolume(int volume, int timestamp)
    {
        var userId = UserId;

        if (volume is < 0 or > 100)
        {
            throw new HubException("Volume must be between 0 and 100");
        }

        await dbContext
            .PlaybackQueues.Where(p => p.Id == userId)
            .ExecuteUpdateAsync(p => p.SetProperty(q => q.Volume, volume));

        await Clients
            .GroupExcept(UserIdentifier, ConnectionId)
            .SendAsync("Volume", new { volume, timestamp });
    }

    public async Task SelectDevice(EntityId deviceId, int timestamp)
    {
        var userId = UserId;

        await dbContext
            .PlaybackQueues.Where(p => p.Id == userId)
            .ExecuteUpdateAsync(p => p.SetProperty(q => q.DeviceId, deviceId));

        await Clients
            .GroupExcept(UserIdentifier, ConnectionId)
            .SendAsync("DeviceSelected", new { deviceId, timestamp });
    }

    // public async Task GetDevices()
    // {
    //     var userId = UserId;
    //
    //     var devices = await dbContext
    //         .Devices.Where(d => d.UserId == userId)
    //         .Select(d => new { d.Id, d.Name })
    //         .ToArrayAsync();
    //
    //     await Clients.Group(UserIdentifier).SendAsync("Devices", devices);
    // }

    public async Task Seek(int position)
    {
        var userId = UserId;

        if (position < 0)
        {
            throw new HubException(
                "Position must be greater than or equal to 0"
            );
        }

        await dbContext
            .PlaybackQueues.Where(p => p.Id == userId)
            .ExecuteUpdateAsync(p => p.SetProperty(q => q.Timestamp, position));

        await Clients
            .GroupExcept(UserIdentifier, ConnectionId)
            .SendAsync("Seek", new { position });
    }

    public async Task Sync(int timestamp)
    {
        var userId = UserId;

        await dbContext
            .PlaybackQueues.Where(p => p.Id == userId)
            .ExecuteUpdateAsync(p =>
                p.SetProperty(q => q.Timestamp, timestamp)
            );

        await Clients
            .GroupExcept(UserIdentifier, ConnectionId)
            .SendAsync("Sync", new { timestamp });
    }

    public async Task SetIsRepeat(bool isRepeat)
    {
        var userId = UserId;

        await dbContext
            .PlaybackQueues.Where(p => p.Id == userId)
            .ExecuteUpdateAsync(p => p.SetProperty(q => q.IsRepeat, isRepeat));

        await Clients
            .GroupExcept(UserIdentifier, ConnectionId)
            .SendAsync("IsRepeat", new { isRepeat });
    }

    public async Task SetIsRandom(bool isRandom)
    {
        var userId = UserId;

        await dbContext
            .PlaybackQueues.Where(p => p.Id == userId)
            .ExecuteUpdateAsync(p => p.SetProperty(q => q.IsRandom, isRandom));

        await Clients
            .GroupExcept(UserIdentifier, ConnectionId)
            .SendAsync("IsRandom", new { isRandom });
    }

    public async Task AddEntry(EntityId trackId)
    {
        var userId = UserId;

        var queue = await dbContext
            .PlaybackQueues.Where(p => p.Id == userId)
            .FirstOrDefaultAsync();

        if (queue is null)
        {
            throw new HubException("User does not have a playback queue");
        }

        var entry = new QueueEntry
        {
            QueueId = queue.Id,
            TrackId = trackId,
            Index = queue.CurrentIndex,
        };

        dbContext.QueueEntries.Add(entry);

        await dbContext.SaveChangesAsync();

        await Clients
            .GroupExcept(UserIdentifier, ConnectionId)
            .SendAsync(
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

        var entry = await dbContext
            .QueueEntries.Where(e =>
                e.Id == entryId && e.Queue.UserId == userId
            )
            .ExecuteDeleteAsync();

        if (entry == 0)
            return;

        await Clients
            .GroupExcept(UserIdentifier, ConnectionId)
            .SendAsync("EntryRemoved", new { entryId });
    }

    public async Task PermuteQueue(int[] newIndices)
    {
        var userId = UserId;

        var queue = await dbContext
            .PlaybackQueues.Where(p => p.Id == userId)
            .FirstOrDefaultAsync();

        if (queue is null)
        {
            throw new HubException("User does not have a playback queue");
        }

        var entries = await dbContext
            .QueueEntries.Where(e => e.QueueId == queue.Id)
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

        await Clients
            .GroupExcept(UserIdentifier, ConnectionId)
            .SendAsync(
                "QueuePermuted",
                entries.Select(e => new { e.Id, e.Index })
            );
    }

    public async Task CleanPlay(EntityId[] trackIds)
    {
        var userId = UserId;

        var queue = await dbContext
            .PlaybackQueues.Where(p => p.Id == userId)
            .FirstOrDefaultAsync();

        if (queue is null)
        {
            throw new HubException("User does not have a playback queue");
        }

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
                Index = queue.TrackCount++,
            };

            dbContext.QueueEntries.Add(entry);
        }

        await dbContext.SaveChangesAsync();

        await Clients
            .GroupExcept(UserIdentifier, ConnectionId)
            .SendAsync("CleanPlay", new { trackIds });
    }

    public async Task SetCurrentTrack(EntityId entryId)
    {
        var userId = UserId;

        var entry = await dbContext
            .QueueEntries.Where(e =>
                e.Id == entryId && e.Queue.UserId == userId
            )
            .FirstOrDefaultAsync();

        if (entry is null)
        {
            throw new HubException("Entry does not exist");
        }

        await dbContext
            .PlaybackQueues.Where(p => p.Id == userId)
            .ExecuteUpdateAsync(p =>
                p.SetProperty(q => q.CurrentIndex, entry.Index)
            );

        await Clients
            .GroupExcept(UserIdentifier, ConnectionId)
            .SendAsync("CurrentTrack", new { entryId });
    }

    public async Task SetDevicePlayback(EntityId deviceId)
    {
        var userId = UserId;

        var queue = await dbContext
            .PlaybackQueues.Where(p => p.Id == userId)
            .FirstOrDefaultAsync();

        if (queue is null)
        {
            throw new HubException("User does not have a playback queue");
        }

        queue.DeviceId = deviceId;
        await dbContext.SaveChangesAsync();

        await Clients
            .GroupExcept(UserIdentifier, ConnectionId)
            .SendAsync("DevicePlayback", new { deviceId });
    }

    public async Task ReorderEntry(int newIndex, int oldIndex)
    {
        var userId = UserId;

        var entry = await dbContext
            .QueueEntries.Where(e =>
                e.Index == oldIndex && e.Queue.UserId == userId
            )
            .FirstOrDefaultAsync();

        if (entry is null)
        {
            throw new HubException("Entry does not exist");
        }

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

        await Clients
            .GroupExcept(UserIdentifier, ConnectionId)
            .SendAsync(
                "EntriesReordered",
                new { entryId = entry.Id, newIndex }
            );
    }

    public async Task Join(string deviceName)
    {
        var userId = UserIdentifier;
        var connectionId = Context.ConnectionId;

        var newDevice = new Device
        {
            UserId = UserId,
            Name = deviceName,
            ConnectionId = connectionId,
        };

        dbContext.Devices.Add(newDevice);
        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "User({userId}) with connection({connectionId}) joined as {deviceName}",
            userId,
            connectionId,
            deviceName
        );

        await Clients
            .GroupExcept(userId, connectionId)
            .SendAsync(
                "DeviceJoined",
                new { deviceName, deviceId = newDevice.Id }
            );
    }
}
