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
            .MapEndpoint<DeleteTrack>()
            .MapEndpoint<GetTrackById>()
            .MapEndpoint<IsTranscoded>()
            .MapEndpoint<SearchTrack>()
            .MapEndpoint<UpdateTrack>();
    }
}
