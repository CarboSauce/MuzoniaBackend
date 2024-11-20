namespace Muzonia.Api.Features.Arist;

public static class MapArtists
{
    public static void MapArtistEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/artists")
            .WithTags("Artists")
            .WithDescription("Endpoints to manage artists");

        endpoints
            .MapAuthorizedGroup()
            .MapEndpoint<DeleteArtist>()
            .MapEndpoint<CreateArtist>()
            .MapEndpoint<GetMyArtists>()
            .MapEndpoint<GetArtist>()
            .MapEndpoint<UpdateArtist>();
    }
}
