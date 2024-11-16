using Muzonia.Api.Hubs;

namespace Muzonia.Api.DepInjection;

internal static class Hubs
{
    public static IApplicationBuilder AddHubs(
        this IApplicationBuilder app,
        IEndpointRouteBuilder route,
        IWebHostEnvironment env
    )
    {
        route.MapGroup("/hub").MapHub<PlayerHub>("/player");
        return app;
    }
}
