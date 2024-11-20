using Microsoft.AspNetCore.Http.HttpResults;
using Muzonia.Core.Dto.Request;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Arist;

public class UpdateArtist : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{id}", Handle);

    private static async Task<
        Results<Ok<ArtistResponse>, BadRequest, ForbidHttpResult, NotFound>
    > Handle(Guid id, ArtistService artistService, UpdateArtistRequest request)
    {
        var artist = await artistService.UpdateArtist(id, request);
        return TypedResults.Ok(artist);
    }
}
