using Microsoft.AspNetCore.Http.HttpResults;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Album;

public class GetAlbums : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id}", Handle);

    private static async Task<
        Results<Ok<AlbumResponse[]>, ForbidHttpResult>
    > Handle(AlbumService albumService, Guid id)
    {
        var albums = await albumService.GetArtistAlbums(id);
        return TypedResults.Ok(albums);
    }
}
