using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Api.Services;
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

    public static async Task<EntityId> DeletePlaylistAsync(
        [ID<Playlist>] EntityId id,
        ApiDbContext dbContext,
        ClaimsPrincipal claims,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;

        if (
            await dbContext.Playlists.AnyAsync(
                p => p.UserId == userId && p.Id == id,
                cancellationToken: ct
            )
        )
        {
            throw new GraphQLException(
                $"Failed to delete playlist because {userId} is not an owner"
            );
        }

        await dbContext.Playlists.Where(p => p.Id == id).ExecuteDeleteAsync(ct);

        return id;
    }

    public static async Task<EntityId> EditPlaylistAsync(
        [ID<Playlist>] EntityId id,
        ReplaceInput<string> name,
        ReplaceInput<string> description,
        ReplaceInput<bool> isPublic,
        ApiDbContext dbContext,
        ClaimsPrincipal claims,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;
        var playlist = await dbContext.Playlists.SingleOrDefaultAsync(
            p => p.UserId == userId && p.Id == id,
            cancellationToken: ct
        );

        if (playlist is null)
        {
            throw new GraphQLException($"Cannot find playlist with id {id}");
        }

        if (name.Replace)
        {
            playlist.Name = name.Value!;
        }

        if (description.Replace)
        {
            playlist.Description = description.Value!;
        }

        if (isPublic.Replace)
        {
            playlist.IsPublic = isPublic.Value;
        }

        await dbContext.SaveChangesAsync(ct);

        return id;
    }

    public static async Task<Uri> CreatePlaylistImageUploadUriAsync(
        [ID<Playlist>] EntityId id,
        ClaimsPrincipal claims,
        BlobServiceClient blobClient,
        ApiDbContext dbContext,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;
        var playlist = await dbContext.Playlists.FirstOrDefaultAsync(
            p => p.Id == id && p.UserId == userId,
            ct
        ) ?? throw new GraphQLException("Playlist not found");

        var blobName = $"playlists/{playlist.Id}";
        var container = blobClient.GetBlobContainerClient("playlists");
        await container.CreateIfNotExistsAsync(cancellationToken: ct);
        var blob = container.GetBlobClient(blobName);
        var sas = new BlobSasBuilder
        {
            BlobContainerName = "playlists",
            BlobName = blobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-1),
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10),
        };
        sas.SetPermissions(
            BlobContainerSasPermissions.Write | BlobContainerSasPermissions.Create
        );
        return blob.GenerateSasUri(sas);
    }

    public static async Task<bool> FinalizePlaylistImageUploadAsync(
        [ID<Playlist>] EntityId id,
        ClaimsPrincipal claims,
        BlobServiceClient blobClient,
        ApiDbContext dbContext,
        FunctionsClient client,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;
        var playlist = await dbContext.Playlists.FirstOrDefaultAsync(
            p => p.Id == id && p.UserId == userId,
            ct
        ) ?? throw new GraphQLException("Playlist not found");

        var blob = blobClient
            .GetBlobContainerClient("playlists")
            .GetBlobClient($"playlists/{playlist.Id}");
        if (!await blob.ExistsAsync(ct))
        {
            throw new GraphQLException("Playlist image blob not found — upload first");
        }

        var resp = await client.HttpClient.PostAsJsonAsync(
            "api/OnPlaylistImageUploaded",
            new { playlistId = playlist.Id.ToString() },
            ct
        );
        resp.EnsureSuccessStatusCode();
        return true;
    }
}

public record CreatePlaylistInput(
    string Name,
    string Description,
    bool IsPublic
);

public class CreatePlaylistValidator : AbstractValidator<CreatePlaylistInput>
{
    public CreatePlaylistValidator()
    {
        RuleFor(x => x.Name).NotNull().MinimumLength(3).NotEmpty();
    }
}
