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

    [UsePaging]
    [UseSorting]
    public static async Task<Page<PlaylistTrack>> GetEntriesAsync(
        [Parent(requires: nameof(Playlist.Id))] Playlist playlist,
        PagingArguments pagingArguments,
        QueryContext<PlaylistTrack> queryContext,
        IPlaylistTracksByPlaylistIdDataLoader dataLoader,
        ClaimsPrincipal claims,
        CancellationToken ct
    )
    {
        return await dataLoader
            .With(pagingArguments, queryContext)
            .LoadRequiredAsync(playlist.Id, ct);
    }
}
