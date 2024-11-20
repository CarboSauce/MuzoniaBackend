using Microsoft.AspNetCore.Http.HttpResults;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Arist;

public class GetMyArtists : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/me", Handle);

    private static async Task<Ok<IEnumerable<ArtistResponse>>> Handle(
        ArtistService artistService
    )
    {
        var artists = await artistService.GetMyArtists();
        return TypedResults.Ok(artists);
    }
}
