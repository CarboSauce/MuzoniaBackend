using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Muzonia.Core.Dto.Request;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Artist;

public class CreateArtistForUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/{userId}", Handle);

    [Authorize(Roles = "Admin")]
    public static async Task<
        Results<Ok<ArtistResponse>, BadRequest, ForbidHttpResult, NotFound>
    > Handle(
        ArtistService artistService,
        EntityId userId,
        [FromForm] CreateArtistRequest request
    )
    {
        var artist = await artistService.CreateArtist(userId, request);
        return TypedResults.Ok(artist);
    }
}
