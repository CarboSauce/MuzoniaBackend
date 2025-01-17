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
            .MapEndpoint<DeleteArtist>()
            .MapEndpoint<CreateArtist>()
            .MapEndpoint<GetMyArtists>()
            .MapEndpoint<GetArtist>()
            .MapEndpoint<GetEntries>()
            .MapEndpoint<UpdateArtist>()
            .MapEndpoint<GetArtistTracks>();
    }
}
