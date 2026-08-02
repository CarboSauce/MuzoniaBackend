using GreenDonut.Data;
using Microsoft.EntityFrameworkCore;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Albums;

public static class AlbumDataLoaders
{
    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Album>
    > AlbumByArtistIdAsync(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        ISelectorBuilder selector,
        CancellationToken ct
    )
    {
        return await dbContext
            .ArtistAlbums.Where(a => ids.Contains(a.ArtistId))
            .Select(a => a.AlbumId, selector)
            .ToDictionaryAsync(a => a.ArtistId, a => a.Album, ct);
    }

    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, Album>
    > AlbumByIdAsync(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        ISelectorBuilder selector,
        CancellationToken ct
    )
    {
        return await dbContext
            .Albums.Where(a => ids.Contains(a.Id))
            .Select(a => a.Id, selector)
            .ToDictionaryAsync(a => a.Id, ct);
    }
}
