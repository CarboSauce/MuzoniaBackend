using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Muzonia.Core.Dto.Request;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Artist;

public class UpdateArtist : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPatch("/{id}", Handle);

    public static async Task<
        Results<Ok<ArtistResponse>, BadRequest, ForbidHttpResult, NotFound>
    > Handle(
        Guid id,
        ArtistService artistService,
        [FromForm] UpdateArtistRequest request
    )
    {
        var artist = await artistService.UpdateArtist(id, request);
        return TypedResults.Ok(artist);
    }
}
