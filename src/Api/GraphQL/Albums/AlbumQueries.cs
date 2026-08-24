using System.Security.Claims;
using GreenDonut.Data;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.GraphQL.Artists;
using Muzonia.Api.GraphQL.Tracks;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Albums;

[QueryType]
public static partial class AlbumQueries
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<Album> GetAlbums(
        ApiDbContext dbContext,
        ClaimsPrincipal user
    ) => dbContext.Albums.AsNoTracking().OrderBy(a => a.Id);

    [NodeResolver]
    public static Task<Album?> GetAlbumByIdAsync(
        EntityId id,
        IAlbumByIdDataLoader dataLoader,
        ISelection selection,
        CancellationToken ct
    ) => dataLoader.Select(selection).LoadAsync(id, ct);

    public static async Task<IEnumerable<Album>> GetAlbumsByIdAsync(
        [ID<Album>] EntityId[] ids,
        IAlbumByIdDataLoader dataLoader,
        ISelection selection,
        CancellationToken ct
    ) => await dataLoader.Select(selection).LoadRequiredAsync(ids, ct);

    public static async Task<IEnumerable<Album>> SearchAlbumsAsync(
        string text,
        ApiDbContext dbContext,
        QueryContext<Album> queryContext,
        CancellationToken ct
    )
    {
        return await dbContext
            .Albums.Where(a =>
                EF.Functions.ToTsVector("english", a.Title).Matches(text)
            )
            .With(queryContext)
            .ToListAsync(ct);
    }
}
