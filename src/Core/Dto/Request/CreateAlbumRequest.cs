using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Muzonia.Core.Common;

namespace Muzonia.Core.Dto.Request;

public record CreateAlbumRequest(string Name, IFormFile File, Guid[] ArtistIds);
