using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Tracks;

[ObjectType<Track>]
public static partial class TrackType
{
    static partial void Configure(IObjectTypeDescriptor<Track> descriptor)
    {
        descriptor.Field(t => t.DataUri).Ignore();
    }

    public static async Task<Uri?> GetDataUri(
        [Parent(requires: $"{nameof(Track.Id)} {nameof(Track.DataUri)}")]
            Track track,
        BlobServiceClient blobClient
    )
    {
        if (track.DataUri is null)
        {
            return null;
        }
        var container = blobClient.GetBlobContainerClient("music");

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = container.Name,
            Resource = "c",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-1),
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(12),
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var baseUri = container.GenerateSasUri(sasBuilder);

        var blobUri = new UriBuilder(baseUri)
        {
            Path =
                $"{baseUri.AbsolutePath.TrimEnd('/')}/{track.Id}/master.m3u8",
        }.Uri;

        return blobUri;
    }
}
