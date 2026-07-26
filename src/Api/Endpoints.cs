using EntityFramework.Exceptions.Common;
using Muzonia.Api.DepInjection;
using Muzonia.Api.Features.Account;
using Muzonia.Api.Features.Album;
using Muzonia.Api.Features.Artist;
using Muzonia.Api.Features.Playback;
using Muzonia.Api.Features.Playlists;
using Muzonia.Api.Features.Tracks;
using Muzonia.Api.Features.User;
using Muzonia.Api.Hubs;
using Muzonia.Api.Middleware;
using Muzonia.Core.Exceptions;

namespace Muzonia.Api;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        app.AddExceptionFilter();

        var root = app.MapGroup("").DisableAntiforgery();

        root.MapHub<PlayerHub>("/player");
        root.MapAccountEndpoints();
        root.MapUserEndpoints();
        root.MapArtistEndpoints();
        root.MapAlbumEndpoints();
        root.MapTracksEndpoints();
        root.MapPlaylistEndpoints();
        root.MapPlaybackEndpoints();
    }

    public static RouteHandlerBuilder WithValidation<T>(
        this RouteHandlerBuilder builder
    ) =>
        builder
            .AddEndpointFilter<RequestValidationAsyncFilter<T>>()
            .ProducesValidationProblem();
}
