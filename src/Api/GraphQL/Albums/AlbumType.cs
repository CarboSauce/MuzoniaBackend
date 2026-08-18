using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using GreenDonut.Data;
using HotChocolate.Execution;
using Muzonia.Api.GraphQL.Tracks;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Albums;

[ObjectType<Album>]
public static partial class AlbumType
{
    static partial void Configure(IObjectTypeDescriptor<Album> descriptor)
    {
        descriptor.Field(u => u.ImageUri).Ignore();
    }

    [UsePaging]
    [UseSorting]
    [BindMember(nameof(Album.Tracks))]
    public static async Task<Page<Track>?> GetTracksAsync(
        [Parent(requires: nameof(Album.Id))] Album album,
        ITracksByAlbumIdDataLoader dataLoader,
        PagingArguments paging,
        QueryContext<Track>? query,
        CancellationToken ct
    ) => await dataLoader.With(paging, query).LoadAsync(album.Id, ct);

    public static string GetImageUri(
        [Parent(requires: $"{nameof(Album.Id)}")] Album artist,
        BlobServiceClient blobClient
    )
    {
        if (artist.ImageUri is null)
        {
            throw new ArgumentNullException(nameof(artist.ImageUri));
        }
        var container = blobClient.GetBlobContainerClient("albums");
        var blobName = $"{artist.Id}.jpg";
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
}
