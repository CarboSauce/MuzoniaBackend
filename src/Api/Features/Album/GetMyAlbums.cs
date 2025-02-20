using Microsoft.AspNetCore.Http.HttpResults;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Album;

public class GetMyAlbums : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/", Handle);

    public static async Task<
        Results<Ok<IEnumerable<AlbumResponse>>, ForbidHttpResult>
    > Handle(AlbumService albumService)
    {
        var albums = await albumService.GetMyAlbums();
        return TypedResults.Ok(albums);
    }
}
