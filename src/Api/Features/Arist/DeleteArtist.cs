using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Arist;

public class DeleteArtist : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/{id}", Handle);

    [Authorize(Roles = "Admin")]
    private static async Task<
        Results<Ok, BadRequest, ForbidHttpResult, NotFound>
    > Handle(HttpContext context, ArtistService artistService, Guid id)
    {
        await artistService.DeleteArtist(id);
        return TypedResults.Ok();
    }
}
