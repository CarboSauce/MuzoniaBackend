using System.Security.Claims;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Api.Services;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Artists;

[MutationType]
public static class ArtistMutations
{
    public static async Task<Artist> CreateArtist(
        CreateArtistInput input,
        ApiDbContext dbContext,
        ClaimsPrincipal user
    )
    {
        var userId = user.UserId;

        if (await dbContext.Artists.AnyAsync(a => a.Name == input.Name))
        {
            throw new GraphQLException("Artist with such name already exists");
        }
        if (await dbContext.Artists.AnyAsync(a => a.UserId == userId))
        {
            throw new GraphQLException("You already have an artist profile");
        }

        var artist = new Artist
        {
            UserId = userId,
            Name = input.Name,
            Description = input.Description,
            ImageUri = null,
        };

        await dbContext.Artists.AddAsync(artist);
        await dbContext.SaveChangesAsync();

        return artist;
    }

    public static async Task<EntityId> DeleteMyArtist(
        ApiDbContext dbContext,
        ClaimsPrincipal user
    )
    {
        var userId = user.UserId;

        var artist = await dbContext.Artists.SingleOrDefaultAsync(a =>
            a.UserId == userId
        );

        if (artist != null)
        {
            dbContext.Artists.Remove(artist);
        }

        await dbContext.SaveChangesAsync();

        return artist?.Id ?? EntityId.Empty;
    }

    public static async Task<Artist> EditMyArtist(
        ReplaceInput<string> name,
        ReplaceInput<string> description,
        ApiDbContext dbContext,
        ClaimsPrincipal user
    )
    {
        var userId = user.UserId;

        var artist = await dbContext.Artists.SingleOrDefaultAsync(a =>
            a.UserId == userId
        );

        if (artist == null)
            throw new GraphQLException("You don't have an artist profile");

        if (name.Replace)
            artist.Name = name.Value;

        if (description.Replace)
            artist.Description = description.Value;

        await dbContext.SaveChangesAsync();

        return artist;
    }

    public static async Task<Uri> CreateArtistImageUploadUriAsync(
        ClaimsPrincipal claims,
        BlobServiceClient blobClient,
        ApiDbContext dbContext,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;
        var artist =
            await dbContext.Artists.FirstOrDefaultAsync(
                a => a.UserId == userId,
                ct
            ) ?? throw new GraphQLException("Artist profile not found");

        var blobName = $"artists/{artist.Id}";
        var container = blobClient.GetBlobContainerClient("artists");
        await container.CreateIfNotExistsAsync(cancellationToken: ct);
        var blob = container.GetBlobClient(blobName);
        var sas = new BlobSasBuilder
        {
            BlobContainerName = "artists",
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

    public static async Task<bool> FinalizeArtistImageUploadAsync(
        ClaimsPrincipal claims,
        BlobServiceClient blobClient,
        ApiDbContext dbContext,
        FunctionsClient client,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;
        var artist =
            await dbContext.Artists.FirstOrDefaultAsync(
                a => a.UserId == userId,
                ct
            ) ?? throw new GraphQLException("Artist profile not found");

        var blob = blobClient
            .GetBlobContainerClient("artists")
            .GetBlobClient($"artists/{artist.Id}");
        if (!await blob.ExistsAsync(ct))
        {
            throw new GraphQLException(
                "Artist image blob not found — upload first"
            );
        }

        var resp = await client.HttpClient.PostAsJsonAsync(
            "api/OnArtistImageUploaded",
            new { artistId = artist.Id.ToString() },
            ct
        );
        resp.EnsureSuccessStatusCode();
        return true;
    }
}

public record CreateArtistInput(string Name, string Description);
