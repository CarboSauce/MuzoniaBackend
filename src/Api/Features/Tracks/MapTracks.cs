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
            .MapEndpoint<GetAlbumTracks>()
            .MapEndpoint<GetArtistTracks>()
            .MapEndpoint<GetMineTranscoding>()
            .MapEndpoint<GetTrackById>()
            .MapEndpoint<IsTranscoded>()
            .MapEndpoint<SearchTrack>()
            .MapEndpoint<UpdateTrack>();
    }
}
