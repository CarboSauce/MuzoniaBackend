using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Muzonia.Core.Dto.Request;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Album;

public class CreateAlbum : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", Handle).WithDescription("Create album");

    public static async Task<
        Results<Ok<AlbumResponse>, BadRequest, ForbidHttpResult, NotFound>
    > Handle(
        HttpContext context,
        AlbumService albumService,
        [FromForm] CreateAlbumRequest request
    )
    {
        var album = await albumService.CreateAlbum(request);
        return TypedResults.Ok(album);
    }
}
