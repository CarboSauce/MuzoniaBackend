using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Muzonia.Core.Dto.Request;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Arist;

public class CreateArtist : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", Handle);

    [Authorize(Roles = "Admin")]
    private static async Task<
        Results<Ok<ArtistResponse>, BadRequest, ForbidHttpResult, NotFound>
    > Handle(ArtistService artistService, CreateArtistRequest request)
    {
        var artist = await artistService.CreateArtist(request);
        return TypedResults.Ok(artist);
    }
}
