using System.Net.Http.Json;
using System.Security.Claims;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using Muzonia.Api.Common;
using Muzonia.Api.Services;

namespace Muzonia.Api.GraphQL.Users;

[MutationType]
public static class UserMutations
{
    public static async Task<Uri> CreateUserAvatarUploadUriAsync(
        ClaimsPrincipal claims,
        BlobServiceClient blobClient,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;
        var blobName = $"users/{userId}";
        var container = blobClient.GetBlobContainerClient("users");
        await container.CreateIfNotExistsAsync(cancellationToken: ct);
        var blob = container.GetBlobClient(blobName);
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = "users",
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

    public static async Task<bool> FinalizeUserAvatarUploadAsync(
        ClaimsPrincipal claims,
        BlobServiceClient blobClient,
        IConfiguration config,
        FunctionsClient client,
        CancellationToken ct
    )
    {
        var userId = claims.UserId;
        var blob = blobClient
            .GetBlobContainerClient("users")
            .GetBlobClient($"users/{userId}");

        if (!await blob.ExistsAsync(ct))
        {
            throw new GraphQLException("Avatar blob not found — upload first");
        }

        var url = "api/OnAvatarUploaded";

        var resp = await client.HttpClient.PostAsJsonAsync(
            url,
            new { userId = userId.ToString() },
            ct
        );
        resp.EnsureSuccessStatusCode();
        return true;
    }
}
