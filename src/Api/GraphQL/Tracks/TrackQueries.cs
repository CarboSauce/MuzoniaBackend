using System.Security.Claims;
using GreenDonut.Data;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
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

    public static async Task<IEnumerable<Track>> SearchTracksAsync(
        string title,
        ApiDbContext dbContext,
        CancellationToken ct
    )
    {
        return await dbContext
            .Tracks.Where(t =>
                t.DataUri != null
                && EF.Functions.ToTsVector(t.Title).Matches(title)
            )
            .ToListAsync(ct);
    }

    public static async Task<Track[]> GetMineTranscodingTracksAsync(
        ApiDbContext dbContext,
        ClaimsPrincipal claims,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;
        var artist = await dbContext.Users.GetArtist(userId).SingleAsync(ct);
        if (artist is null)
        {
            throw new GraphQLException("User is not an artist");
        }

        var tracks = await dbContext
            .Tracks.Where(t =>
                t.PrimaryArtistId == artist.Id && t.DataUri == null
            )
            .ToArrayAsync(ct);

        return tracks;
    }
}
