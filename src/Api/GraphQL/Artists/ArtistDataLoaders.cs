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
}
