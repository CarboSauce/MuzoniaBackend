using Muzonia.DbEf.Entities;

namespace Muzonia.Core.Dto.Response;

public record ArtistResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string Description,
    Uri ImageUri,
    DateTime CreationDate
)
{
    public ArtistResponse(Artist artist)
        : this(
            Id: artist.Id,
            UserId: artist.UserId,
            Name: artist.Name,
            Description: artist.Description,
            ImageUri: artist.ImageUri,
            CreationDate: artist.CreationDate
        ) { }
}
