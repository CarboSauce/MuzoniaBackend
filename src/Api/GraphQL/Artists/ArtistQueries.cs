using System.Security.Claims;
using GreenDonut.Data;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Artists;

[QueryType]
public static partial class ArtistQueries
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<Artist> GetArtist(
        ApiDbContext dbContext,
        ClaimsPrincipal claims
    ) => dbContext.Artists.AsNoTracking().OrderBy(a => a.Id);

    [NodeResolver]
    public static Task<Artist?> GetArtistByIdAsync(
        EntityId id,
        IArtistByIdDataLoader dataLoader,
        ISelection selection,
        CancellationToken ct
    ) => dataLoader.Select(selection).LoadAsync(id, ct);

    public static async Task<IEnumerable<Artist>> GetArtistsByIdAsync(
        [ID<Artist>] EntityId[] ids,
        IArtistByIdDataLoader dataLoader,
        ISelection selection,
        CancellationToken ct
    ) => await dataLoader.Select(selection).LoadRequiredAsync(ids, ct);

    public static async Task<IEnumerable<Artist>> SearchArtistsAsync(
        ApiDbContext dbContext,
        ClaimsPrincipal claims,
        QueryContext<Artist> queryContext,
        string text,
        CancellationToken ct
    )
    {
        return await dbContext
            .Artists.Where(a =>
                EF.Functions.ToTsVector("english", a.Description + " " + a.Name)
                    .Matches(text)
            )
            .With(queryContext)
            .ToListAsync(ct);
    }
}
