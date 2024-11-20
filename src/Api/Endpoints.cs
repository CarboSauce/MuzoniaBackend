using EntityFramework.Exceptions.Common;
using Muzonia.Api.Features.Account;
using Muzonia.Api.Features.Album;
using Muzonia.Api.Features.Arist;
using Muzonia.Api.Features.User;
using Muzonia.Api.Middleware;
using Muzonia.Core.Exceptions;

namespace Muzonia.Api;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        app.AddExceptionFilter();

        var root = app.MapGroup("").WithOpenApi();

        root.MapAccountEndpoints();
        root.MapUserEndpoints();
        root.MapArtistEndpoints();
        root.MapAlbumEndpoints();
    }
}
