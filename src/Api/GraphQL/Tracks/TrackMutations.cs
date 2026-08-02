using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Tracks;

[MutationType]
public static class TrackMutations
{
    public static async Task<Track> CreateTrack(
        CreateTrackInput input,
        ApiDbContext dbContext,
        ClaimsPrincipal user
    )
    {
        var userId = user.UserId;

        var artist = await dbContext
            .Users.GetArtist(userId)
            .FirstOrDefaultAsync();

        if (artist is null)
        {
            throw new GraphQLException("User is not an artist");
        }

        var track = new Track
        {
            Title = input.Title,
            Genre = input.Genre,
            AlbumId = input.AlbumId,
            DataUri = new Uri("file://nofile"),
            Duration = 0,
            PrimaryArtistId = artist.Id,
        };

        dbContext.Tracks.Add(track);
        dbContext.TrackArtists.Add(
            new TrackArtist() { TrackId = track.Id, ArtistId = artist.Id }
        );

        await dbContext.SaveChangesAsync();

        return track;

        // foreach (var otherArtist in otherArtists)
        // {
        //     if (artist.Id != otherArtist)
        //     {
        //         dbContext.TrackArtists.Add(
        //             new() { TrackId = track.Id, ArtistId = otherArtist }
        //         );
        //     }
        // }
    }
}

public record CreateTrackInput(string Title, string Genre, EntityId? AlbumId);

// public record CreateTrackPayload(
//     string Title,
//     string Genre,
//     EntityId Id,
//     DateTime CreationDate,
//     EntityId? AlbumId,
//     EntityId PrimaryArtistId,
//     EntityId[] OtherArtistIds,
//     string TranscodingId
// );
