using Microsoft.AspNetCore.Http;

namespace Muzonia.Core.Dto.Request;

public record CreateAlbumRequest(
    string Name,
    string Description,
    IFormFile File,
    Guid[] ArtistIds
);
