using Microsoft.EntityFrameworkCore;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Playlists;

[MutationType]
public static partial class PlaylistEntriesMutations
{
    public static async Task<PlaylistTrack> AddPlaylistEntryAsync(
        AddPlaylistEntryInput input,
        ApiDbContext dbContext,
        CancellationToken ct
    )
    {
        var playlist = await dbContext.Playlists.FirstOrDefaultAsync(
            e => e.Id == input.PlaylistId,
            ct
        );

        if (playlist is null)
        {
            throw new GraphQLException("Playlist not found");
        }

        var track = await dbContext.Tracks.FirstOrDefaultAsync(
            e => e.Id == input.TrackId && e.DataUri != null,
            ct
        );

        if (track is null)
        {
            throw new GraphQLException("Track not found");
        }

        var entry = new PlaylistTrack
        {
            PlaylistId = input.PlaylistId,
            TrackId = input.TrackId,
            Index = playlist.TrackCount,
        };

        dbContext.PlaylistTracks.Add(entry);
        playlist.TrackCount += 1;

        await dbContext.SaveChangesAsync(ct);

        return entry;
    }
}

public record AddPlaylistEntryInput(EntityId PlaylistId, EntityId TrackId);
