using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Muzonia.Core.Dto.Request;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Artist;

public class CreateArtist : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", Handle);

    public static async Task<
        Results<Ok<ArtistResponse>, BadRequest, ForbidHttpResult, NotFound>
    > Handle(
        ArtistService artistService,
        UserService userService,
        [FromForm] CreateArtistRequest request
    )
    {
        var user = await userService.GetUser();
        var artist = await artistService.CreateArtist(user.Id, request);
        return TypedResults.Ok(artist);
    }
}
