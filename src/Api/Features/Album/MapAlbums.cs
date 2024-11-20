namespace Muzonia.Api.Features.Album;

public static class MapAlbums
{
    public static void MapAlbumEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/albums")
            .WithTags("Albums")
            .WithDescription("Endpoints to manage albums");

        endpoints
            .MapAuthorizedGroup()
            .MapEndpoint<CreateAlbum>()
            //.MapEndpoint<DeleteAlbum>()
            .MapEndpoint<GetMyAlbums>()
            .MapEndpoint<GetAlbums>();
        //.MapEndpoint<UpdateAlbum>();
    }
}
