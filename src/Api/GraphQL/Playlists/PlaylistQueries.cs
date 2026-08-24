using System.Security.Claims;
using GreenDonut.Data;
using HotChocolate.Execution;
using HotChocolate.Types.Pagination;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Playlists;

[QueryType]
public static partial class PlaylistQueries
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<Playlist> GetPlaylists(
        ApiDbContext dbContext,
        ClaimsPrincipal claims
    )
    {
        var userId = claims.UserId;
        return dbContext
            .Playlists.AsNoTracking()
            .Where(p => p.UserId == userId || p.IsPublic)
            .OrderBy(a => a.Id);
    }

    public static async Task<IEnumerable<Playlist>> SearchPlaylistsAsync(
        ApiDbContext dbContext,
        ClaimsPrincipal claims,
        string text,
        QueryContext<Playlist> queryContext,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;
        return await dbContext
            .Playlists.Where(p =>
                (p.IsPublic || p.UserId == userId)
                && EF.Functions.ToTsVector(
                        "english",
                        p.Description + " " + p.Name
                    )
                    .Matches(text)
            )
            .With(queryContext)
            .ToListAsync(ct);
    }

    [NodeResolver]
    public static Task<Playlist?> GetPlaylistByIdAsync(
        EntityId id,
        IPlaylistByIdDataLoader dataLoader,
        ISelection selection,
        ClaimsPrincipal claims,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;
        return dataLoader
            .SetState("userId", userId)
            .Select(selection)
            .LoadAsync(id, ct);
    }
}
