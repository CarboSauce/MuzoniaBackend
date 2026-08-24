using GreenDonut.Data;
using Microsoft.EntityFrameworkCore;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Artists;

public static class ArtistDataLoaders
{
    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Artist>
    > ArtistByUserIdAsync(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        ISelectorBuilder selector,
        CancellationToken ct
    )
    {
        return await dbContext
            .Artists.Where(a => ids.Contains(a.UserId))
            .Select(a => a.UserId, selector)
            .ToDictionaryAsync(s => s.UserId, ct);
    }

    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Artist>
    > ArtistByIdAsync(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        ISelectorBuilder selector,
        CancellationToken ct
    )
    {
        return await dbContext
            .Artists.Where(a => ids.Contains(a.Id))
            .Select(a => a.Id, selector)
            .ToDictionaryAsync(s => s.Id, ct);
    }

    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Artist>
    > ArtistByTrackIdAsync(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        ISelectorBuilder selector,
        CancellationToken ct
    )
    {
        // var artists = await dbContext
        //     .Tracks.Where(t => ids.Contains(t.Id))
        //     .Select(t => new { Id = t.Id, Artist = t.PrimaryArtist })
        //     .ToDictionaryAsync(t => t.Id, t => t.Artist, ct);

        var artists = await dbContext
            .Tracks.AsNoTracking()
            .Where(a => ids.Contains(a.Id))
            .Select(
                t => t.Id,
                t => t.Artists.Where(a => a.Id == t.PrimaryArtistId),
                selector
            )
            .ToDictionaryAsync(t => t.Key, t => t.Value.First(), ct);

        return artists;
    }
}
