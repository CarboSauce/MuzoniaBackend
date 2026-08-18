using System.Security.Claims;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using GreenDonut.Data;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Playlists;

[ObjectType<Playlist>]
public static partial class PlaylistType
{
    static partial void Configure(IObjectTypeDescriptor<Playlist> descriptor)
    {
        descriptor.Field(u => u.ImageUri).Ignore();
        descriptor.Field(u => u.Tracks).Ignore();
    }

    public static string? GetImageUri(
        [Parent(requires: $"{nameof(Playlist.Id)} {nameof(Playlist.ImageUri)}")]
            Playlist playlist,
        BlobServiceClient blobClient
    )
    {
        if (playlist.ImageUri is null)
        {
            return null;
        }
        var container = blobClient.GetBlobContainerClient("playlists");
        var blobName = $"{playlist.Id}.jpg";
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
                .LoadAsync(playlist.Id, ct)
            ?? Page<PlaylistTrack>.Empty;
    }
}
