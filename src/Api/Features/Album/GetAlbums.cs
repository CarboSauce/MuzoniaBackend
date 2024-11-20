using Microsoft.AspNetCore.Http.HttpResults;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Album;

public class GetAlbums : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id}/{pageId}", Handle);

    private static async Task<
        Results<Ok<IEnumerable<AlbumResponse>>, ForbidHttpResult>
    > Handle(AlbumService albumService, Guid id, Guid pageId)
    {
        var albums = await albumService.GetArtistAlbumsPaginate(id, pageId, 30);
        return TypedResults.Ok(albums);
    }
}
