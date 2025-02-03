namespace Muzonia.Api.Features.Playlists;

public static class MapPlaylists
{
    public static void MapPlaylistEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/playlists")
            .WithTags("Playlists")
            .WithDescription("Endpoints to manage playlists");

        endpoints
            .MapAuthorizedGroup()
            .MapEndpoint<AddEntry>()
            .MapEndpoint<CreatePlaylist>()
            .MapEndpoint<DeletePlaylist>()
            .MapEndpoint<EditPlaylistInfo>()
            .MapEndpoint<GetEntries>()
            .MapEndpoint<GetMinePlaylists>()
            .MapEndpoint<GetPlaylist>()
            .MapEndpoint<GetUserPlaylists>()
            .MapEndpoint<RemoveEntry>()
            .MapEndpoint<ReorderEntry>()
            .MapEndpoint<SearchPlaylist>();
    }
}
