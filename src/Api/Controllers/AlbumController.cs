using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Muzonia.Core.Dto.Request;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class AlbumController(AlbumService albumService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAlbum(
        [FromForm] CreateAlbumRequest request
    )
    {
        var album = await albumService.CreateAlbum(request);

        return Ok(album);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyAlbums()
    {
        var albums = await albumService.GetMyAlbums();

        return Ok(albums);
    }

    [HttpGet("{id}/{pageId}")]
    public async Task<IActionResult> GetAlbums(Guid id, Guid pageId)
    {
        var album = await albumService.GetArtistAlbumsPaginate(id, pageId, 30);

        return Ok(album);
    }
}
