namespace Muzonia.Api.Features.Account;

public static class MapAccount
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/accounts")
            .WithTags("Accounts")
            .WithDescription("Endpoints to manage account");

        endpoints
            .MapAnonymousGroup()
            .MapEndpoint<ConfirmEmail>()
            .MapEndpoint<LoginUser>()
            .MapEndpoint<RegisterUser>();

        endpoints.MapAuthorizedGroup().MapEndpoint<LogoutUser>();
    }
}
