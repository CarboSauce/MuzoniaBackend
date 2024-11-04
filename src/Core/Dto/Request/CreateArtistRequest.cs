using Microsoft.AspNetCore.Http;

namespace Muzonia.Core.Dto.Request;

public record CreateArtistRequest(
    Guid UserId,
    string Name,
    string Description,
    IFormFile File
);
