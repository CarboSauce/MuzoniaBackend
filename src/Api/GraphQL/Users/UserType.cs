using System.Security.Claims;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using GreenDonut.Data;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Api.GraphQL.Artists;
using Muzonia.Api.GraphQL.Playlists;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Users;

[ObjectType<AppUser>]
public static partial class UserType
{
    static partial void Configure(IObjectTypeDescriptor<AppUser> descriptor)
    {
        descriptor.Field(u => u.Artist).Ignore();
        descriptor.Field(u => u.Playlists).Ignore();
        descriptor.Field(u => u.ImageUri).Ignore();
        descriptor.Field(u => u.Id).ID<AppUser>();
    }

    public static async Task<Artist?> GetArtistAsync(
        [Parent(requires: nameof(AppUser.Id))] AppUser user,
        IArtistByUserIdDataLoader artistByUserIdDataLoader,
        ISelection selection,
        CancellationToken cancellationToken
    ) =>
        await artistByUserIdDataLoader
            .Select(selection)
            .LoadAsync(user.Id, cancellationToken);

    public static async Task<string?> GetImageUriAsync(
        [Parent(requires: $"{nameof(AppUser.Id)}")] AppUser user,
        BlobServiceClient blobClient
    )
    {
        var container = blobClient.GetBlobContainerClient("users");
        var blobName = $"{user.Id}.jpg";
        var blob = container.GetBlobClient(blobName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = container.Name,
            BlobName = blobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-1),
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(15),
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blob.GenerateSasUri(sasBuilder).ToString();
    }

    [UsePaging]
    [UseSorting]
    public static async Task<Page<Playlist>> GetPlaylistsAsync(
        [Parent(requires: nameof(AppUser.Id))] AppUser user,
        PlaylistsByUserIdDataLoader dataLoader,
        PagingArguments paging,
        QueryContext<Playlist> query,
        ClaimsPrincipal claims,
        CancellationToken ct
    )
    {
        var curUserId = claims.UserId;
        return await dataLoader
                .With(paging, query)
                .SetState("userId", curUserId)
                .LoadAsync(user.Id, ct)
            ?? Page<Playlist>.Empty;
    }
}
