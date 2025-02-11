using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Playlists;

public class GetUserPlaylists : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/user/{userId}", Handle);

    public record Response(
        string Title,
        string Description,
        EntityId Id,
        EntityId UserId,
        DateTime CreationDate,
        bool IsPublic,
        Uri? ImageUri
    );

    private static async Task<Ok<Response[]>> Handle(
        EntityId userId,
        ApiDbContext dbContext,
        UserService userService
    )
    {
        var user = await userService.GetUser();

        var isAdmin = await userService.IsUserAdmin(user);

        var playlistsQuery = dbContext
            .Playlists.Where(e => e.UserId == userId)
            .WhereIf(!isAdmin && userId != user.Id, e => e.IsPublic);

        var playlists = await playlistsQuery
            .Select(e => new Response(
                e.Name,
                e.Description,
                e.Id,
                e.UserId,
                e.CreationDate,
                e.IsPublic,
                e.ImageUri
            ))
            .ToArrayAsync();

        return TypedResults.Ok(playlists);
    }
}
