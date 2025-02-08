using Muzonia.Api.Features.Playlists;

namespace Muzonia.Api.Features.Artist;

public static class MapArtists
{
    public static void MapArtistEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/artists")
            .WithTags("Artists")
            .WithDescription("Endpoints to manage artists");

        endpoints
            .MapAuthorizedGroup()
            .MapEndpoint<CreateArtist>()
            .MapEndpoint<CreateArtistForUser>()
            .MapEndpoint<DeleteArtist>()
            .MapEndpoint<GetArtist>()
            .MapEndpoint<GetArtistAlbums>()
            .MapEndpoint<GetMyArtist>()
            .MapEndpoint<SearchArtist>()
            .MapEndpoint<UpdateArtist>();
    }
}
