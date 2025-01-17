using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Playlists;

public class SearchPlaylist : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/search/{name}", Handle);

    public record Response(
        string Title,
        string Description,
        EntityId Id,
        EntityId UserId,
        DateTime CreationDate,
        bool IsPublic,
        Uri? ImageUri
    );

    private static async Task<Results<Ok<Response[]>, BadRequest>> Handle(
        string name,
        ApiDbContext dbContext,
        UserService userService
    )
    {
        var (user, isAdmin) = await userService.CurrentUser();

        var playlists = await dbContext
            .Playlists.Where(e => EF.Functions.ILike(e.Name, $"%{name}%"))
            .Select(e => new Response(
                e.Name,
                e.Description,
                e.Id,
                e.UserId,
                e.CreationDate,
                e.IsPublic,
                e.ImageUri
            ))
            .Take(50)
            .ToArrayAsync();

        return TypedResults.Ok(playlists);
    }
}
