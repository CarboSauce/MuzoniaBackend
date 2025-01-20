using Muzonia.DbEf.Entities;

namespace Muzonia.Core.Dto.Response;

public record AlbumResponse(
    Guid Id,
    EntityId OwnerId,
    ArtistResponse[] Artists,
    string Title,
    Uri ImageUri,
    DateTime CreationDate
)
{
    public AlbumResponse(Album album, ArtistResponse[] Artists)
        : this(
            Id: album.Id,
            OwnerId: album.OwnerId,
            Artists: Artists,
            Title: album.Title,
            ImageUri: album.ImageUri,
            CreationDate: album.CreationDate
        ) { }
}
