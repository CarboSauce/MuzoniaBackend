using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Muzonia.Core;

public interface IEndpoint
{
    static abstract void Map(IEndpointRouteBuilder app);
}

public static class EndpointExt
{
    public static IEndpointRouteBuilder MapEndpoint<T>(
        this IEndpointRouteBuilder app
    )
        where T : IEndpoint
    {
        T.Map(app);
        return app;
    }

    public static IEndpointRouteBuilder MapAnonymousGroup(
        this IEndpointRouteBuilder app,
        string path = ""
    ) => app.MapGroup(path).AllowAnonymous();

    public static IEndpointRouteBuilder MapAuthorizedGroup(
        this IEndpointRouteBuilder app,
        string path = ""
    ) => app.MapGroup(path).RequireAuthorization();
}
