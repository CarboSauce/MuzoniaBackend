using Microsoft.AspNetCore.Http;

namespace Muzonia.Core.Dto.Request;

public record CreateArtistRequest(
    string Name,
    string Description,
    IFormFile File
);
