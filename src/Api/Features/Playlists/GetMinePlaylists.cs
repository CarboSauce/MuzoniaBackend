using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Playlists;

public class GetMinePlaylists : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/", Handle);

    public record Response(
        string Title,
        string Description,
        EntityId Id,
        EntityId UserId,
        DateTime CreationDate,
        bool IsPublic,
        Uri? ImageUri
    );

    public static async Task<Ok<Response[]>> Handle(
        ApiDbContext dbContext,
        UserService userService
    )
    {
        var user = await userService.GetUser();

        var playlists = await dbContext
            .Playlists.Where(e => e.UserId == user.Id)
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
