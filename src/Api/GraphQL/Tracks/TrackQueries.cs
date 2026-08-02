using System.Security.Claims;
using GreenDonut.Data;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.GraphQL.Artists;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Tracks;

[QueryType]
public static partial class TrackQueries
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<Track> GetTracks(
        ApiDbContext dbContext,
        ClaimsPrincipal claims
    ) => dbContext.Tracks.AsNoTracking().OrderBy(a => a.Id);

    [NodeResolver]
    public static Task<Track?> GetTrackByIdAsync(
        EntityId id,
        ITrackByIdDataLoader dataLoader,
        ISelection selection,
        CancellationToken ct
    ) => dataLoader.Select(selection).LoadAsync(id, ct);

    public static async Task<IEnumerable<Track>> GetTracksByIdAsync(
        [ID<Track>] EntityId[] id,
        ITrackByIdDataLoader dataLoader,
        ISelection selection,
        CancellationToken ct
    ) => await dataLoader.Select(selection).LoadRequiredAsync(id, ct);
}
