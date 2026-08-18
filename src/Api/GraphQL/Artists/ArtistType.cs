using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using GreenDonut.Data;
using HotChocolate.Execution;
using Muzonia.Api.GraphQL.Tracks;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Artists;

[ObjectType<Artist>]
public static partial class ArtistType
{
    static partial void Configure(IObjectTypeDescriptor<Artist> descriptor)
    {
        descriptor.Field(u => u.ImageUri).Ignore();
    }

    [UsePaging]
    [UseSorting]
    [BindMember(nameof(Artist.Tracks))]
    public static async Task<Page<Track>> GetTrackAsync(
        [Parent] Artist artist,
        ITracksByArtistIdDataLoader tracksByArtistIdDataLoader,
        PagingArguments paging,
        QueryContext<Track> queryContext,
        CancellationToken cancellationToken
    ) =>
        await tracksByArtistIdDataLoader
            .With(paging, queryContext)
            .LoadRequiredAsync(artist.Id, cancellationToken);

    public static string? GetImageUri(
        [Parent(requires: $"{nameof(Artist.Id)} {nameof(Artist.ImageUri)}")]
            Artist artist,
        BlobServiceClient blobClient
    )
    {
        if (artist.ImageUri is null)
        {
            return null;
        }
        var container = blobClient.GetBlobContainerClient("artists");
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
