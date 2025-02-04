using Microsoft.AspNetCore.Http;

namespace Muzonia.Core.Dto.Request;

public class UpdateArtistRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public IFormFile? File { get; set; }
}
