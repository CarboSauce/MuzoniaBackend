using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Muzonia.Core.Dto.Request;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ArtistController(ArtistService artistService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<Ok<ArtistResponse>> CreateArtist(
        [FromForm] CreateArtistRequest request
    )
    {
        var artist = await artistService.CreateArtist(request);
        return TypedResults.Ok(artist);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<Ok> DeleteArtist(Guid id)
    {
        await artistService.DeleteArtist(id);
        return TypedResults.Ok();
    }

    [HttpGet("me")]
    public async Task<Ok<IEnumerable<ArtistResponse>>> GetMyArtists()
    {
        var artists = await artistService.GetMyArtists();
        return TypedResults.Ok(artists);
    }

    [HttpPut("{id}")]
    public async Task<Ok<ArtistResponse>> UpdateArtist(
        Guid id,
        [FromForm] UpdateArtistRequest request
    )
    {
        var artist = await artistService.UpdateArtist(id, request);
        return TypedResults.Ok(artist);
    }
}
