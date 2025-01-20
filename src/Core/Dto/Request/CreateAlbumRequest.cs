using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Muzonia.Core.Common;

namespace Muzonia.Core.Dto.Request;

public record CreateAlbumRequest(
    string Name,
    string Description,
    IFormFile File,
    Guid[] ArtistIds
);