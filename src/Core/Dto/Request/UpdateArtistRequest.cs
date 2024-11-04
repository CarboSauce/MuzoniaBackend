using Microsoft.AspNetCore.Http;

namespace Muzonia.Core.Dto.Request;

public record UpdateArtistRequest(
    string? Name,
    string? Description,
    IFormFile? File
);
