using Microsoft.AspNetCore.Http.HttpResults;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Artist;

public class GetMyArtist : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/me", Handle);

    public static async Task<Results<Ok<ArtistResponse>, NotFound>> Handle(
        ArtistService artistService
    )
    {
        var artist = await artistService.GetMyArtist();

        return artist is not null
            ? TypedResults.Ok(artist)
            : TypedResults.NotFound();
    }
}
