using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Muzonia.Api.Common;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Playlists;

[MutationType]
public static partial class PlaylistMutations
{
    public static async Task<Playlist> CreatePlaylistAsync(
        CreatePlaylistInput input,
        ApiDbContext dbContext,
        ClaimsPrincipal claims,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;

        var playlist = new Playlist
        {
            Description = input.Description,
            Name = input.Name,
            ImageUri = null,
            UserId = userId,
            IsPublic = input.IsPublic,
            TrackCount = 0,
        };

        dbContext.Playlists.Add(playlist);
        await dbContext.SaveChangesAsync(ct);

        return playlist;
    }
}

public record CreatePlaylistInput(
    [property: MinLength(3)] string Name,
    string Description,
    bool IsPublic
);
