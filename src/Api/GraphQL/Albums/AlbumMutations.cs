using System.Security.Claims;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Api.Services;
using Muzonia.Core.Exceptions;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Albums;

[MutationType]
public static class AlbumMutations
{
    public static async Task<Album> CreateAlbum(
        CreateAlbumInput input,
        ApiDbContext dbContext,
        ClaimsPrincipal claims,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;

        var artist = await dbContext
            .Artists.Where(a => a.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (artist is null)
        {
            throw new GraphQLException("User is not an artist");
        }

        if (await dbContext.Albums.AnyAsync(a => a.Title == input.Name, ct))
        {
            throw new GraphQLException("Album already exists");
        }

        var album = new Album
        {
            OwnerId = artist.Id,
            Title = input.Name,
            ImageUri = null,
        };

        dbContext.Albums.Add(album);

        dbContext.ArtistAlbums.Add(
            new ArtistAlbum { ArtistId = artist.Id, AlbumId = album.Id }
        );

        await dbContext.SaveChangesAsync(ct);

        return album;
    }

    public static async Task<EntityId> DeleteAlbum(
        [ID<Album>] EntityId id,
        ApiDbContext dbContext,
        ClaimsPrincipal claims,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;

        var album = await dbContext
            .Albums.Where(a => a.OwnerId == userId && a.Id == id)
            .FirstOrDefaultAsync(ct);

        if (album is null)
        {
            throw new GraphQLException("No album found");
        }

        dbContext.Albums.Remove(album);

        await dbContext.SaveChangesAsync(ct);

        return album.Id;
    }

    public static async Task<Uri> CreateAlbumImageUploadUriAsync(
        [ID<Album>] EntityId id,
        ClaimsPrincipal claims,
        BlobServiceClient blobClient,
        ApiDbContext dbContext,
        CancellationToken ct
    )
    {
        var album =
            await dbContext.Albums.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new GraphQLException("Album not found");

        var artist = await dbContext.Artists.FirstOrDefaultAsync(
            a => a.Id == album.OwnerId,
            ct
        );
        if (artist is null || artist.UserId != claims.UserId)
        {
            throw new GraphQLException("Not authorized");
        }

        var blobName = $"albums/{album.Id}";
        var container = blobClient.GetBlobContainerClient("albums");
        await container.CreateIfNotExistsAsync(cancellationToken: ct);
        var blob = container.GetBlobClient(blobName);
        var sas = new BlobSasBuilder
        {
            BlobContainerName = "albums",
            BlobName = blobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-1),
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10),
        };
        sas.SetPermissions(
            BlobContainerSasPermissions.Write
                | BlobContainerSasPermissions.Create
        );
        return blob.GenerateSasUri(sas);
    }

    public static async Task<bool> FinalizeAlbumImageUploadAsync(
        [ID<Album>] EntityId id,
        ClaimsPrincipal claims,
        BlobServiceClient blobClient,
        ApiDbContext dbContext,
        FunctionsClient client,
        CancellationToken ct
    )
    {
        var album =
            await dbContext.Albums.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new GraphQLException("Album not found");

        var artist = await dbContext.Artists.FirstOrDefaultAsync(
            a => a.Id == album.OwnerId,
            ct
        );
        if (artist is null || artist.UserId != claims.UserId)
        {
            throw new GraphQLException("Not authorized");
        }

        var blob = blobClient
            .GetBlobContainerClient("albums")
            .GetBlobClient($"albums/{album.Id}");
        if (!await blob.ExistsAsync(ct))
        {
            throw new GraphQLException(
                "Album image blob not found — upload first"
            );
        }

        var resp = await client.HttpClient.PostAsJsonAsync(
            "api/OnAlbumImageUploaded",
            new { albumId = album.Id.ToString() },
            ct
        );
        resp.EnsureSuccessStatusCode();
        return true;
    }
}

public sealed record CreateAlbumInput(string Name);
