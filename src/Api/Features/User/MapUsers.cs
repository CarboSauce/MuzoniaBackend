namespace Muzonia.Api.Features.User;

public static class MapUsers
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/users")
            .WithTags("Users")
            .WithDescription("Endpoints to manage user");

        endpoints
            .MapAuthorizedGroup()
            .MapEndpoint<DeleteUser>()
            .MapEndpoint<DeleteUserById>()
            .MapEndpoint<EditUserInfo>()
            .MapEndpoint<GetUserBasicInfo>()
            .MapEndpoint<GetUserById>()
            .MapEndpoint<GetUserInfo>()
            .MapEndpoint<SearchUsers>();
    }
}
