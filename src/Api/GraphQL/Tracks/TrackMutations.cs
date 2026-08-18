using System.Security.Claims;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Services.Transcoding;
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
            DataUri = null,
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

    public static async Task<EntityId> DeleteTrackAsync(
        [ID<Track>] EntityId trackId,
        ApiDbContext dbContext,
        ClaimsPrincipal user,
        CancellationToken ct
    )
    {
        var userId = user.UserId;

        var track = await dbContext.Tracks.FirstOrDefaultAsync(
            track =>
                track.Id == trackId
                && track.Artists.Contains(
                    dbContext.Artists.Single(a => a.UserId == userId)
                ),
            ct
        );
        if (track is null)
        {
            throw new GraphQLException("Track not found");
        }

        dbContext.Tracks.Remove(track);

        await dbContext.SaveChangesAsync();

        return track.Id;
    }

    public static async Task<Uri> CreateBlobUriAsync(
        [ID<Track>] EntityId trackId,
        ApiDbContext dbContext,
        ClaimsPrincipal claims,
        BlobServiceClient blobClient,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;

        var artist = await dbContext
            .Users.GetArtist(userId)
            .FirstOrDefaultAsync(ct);

        if (artist is null)
        {
            throw new GraphQLException("User is not an artist");
        }

        if (
            !await dbContext.TrackArtists.AnyAsync(
                ta => ta.ArtistId == artist.Id && ta.TrackId == trackId,
                ct
            )
        )
        {
            throw new GraphQLException("Track doesn't belong to this artist");
        }

        var blobName = $"tracks/{trackId}";
        var container = blobClient.GetBlobContainerClient("tracks");
        await container.CreateIfNotExistsAsync(cancellationToken: ct);
        var blob = container.GetBlobClient(blobName);
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = "tracks",
            BlobName = blobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-1),
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10),
        };
        sasBuilder.SetPermissions(
            BlobContainerSasPermissions.Write
                | BlobContainerSasPermissions.Create
        );

        return blob.GenerateSasUri(sasBuilder);
    }

    public static async Task<string> FinalizeTrackUploadAsync(
        [ID<Track>] EntityId trackId,
        ApiDbContext dbContext,
        ClaimsPrincipal user,
        BlobServiceClient blobClient,
        ITranscodingService transcodingService,
        CancellationToken ct
    )
    {
        var userId = user.UserId;
        var artist = await dbContext
            .Users.GetArtist(userId)
            .FirstOrDefaultAsync(ct);

        if (artist is null)
        {
            throw new GraphQLException("User is not an artist");
        }

        if (
            !await dbContext.TrackArtists.AnyAsync(
                ta => ta.ArtistId == artist.Id && ta.TrackId == trackId,
                ct
            )
        )
        {
            throw new GraphQLException("Track doesn't belong to this artist");
        }

        var blob = blobClient
            .GetBlobContainerClient("tracks")
            .GetBlobClient($"tracks/{trackId}");
        if (!await blob.ExistsAsync(ct))
        {
            throw new GraphQLException("Track blob not found — upload first");
        }

        return await transcodingService.Enqueue(trackId, EntityId.Empty);
    }
}

public sealed record CreateTrackInput(
    string Title,
    string Genre,
    EntityId? AlbumId
);
