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
            .MapEndpoint<GetCurrentQueue>()
            .MapEndpoint<CreateQueueForUser>()
            .MapEndpoint<GetEntries>()
            .MapEndpoint<GetMyQueue>()
            .MapEndpoint<GetQueueState>()
            .MapEndpoint<GetUsers>()
            .MapEndpoint<SyncPlayback>();
    }
}
