namespace Muzonia.Api.Features.Tracks;

public static class MapTracks
{
    public static void MapTracksEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/tracks")
            .WithTags("Tracks")
            .WithDescription("Endpoints to manage tracks");

        endpoints
            .MapAuthorizedGroup()
            .MapEndpoint<CreateTrack>()
            .MapEndpoint<GetTrackById>()
            .MapEndpoint<SearchTrack>();
    }
}
