namespace Muzonia.Api.Features.Playback;

public static class MapPlayback
{
    public static void MapPlaybackEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/playback")
            .WithTags("Playback")
            .WithDescription("Endpoints to gather playback information");

        endpoints
            .MapAuthorizedGroup()
            .MapEndpoint<GetDevices>()
            .MapEndpoint<GetEntries>()
            .MapEndpoint<GetPlaybackState>();
    }
}
