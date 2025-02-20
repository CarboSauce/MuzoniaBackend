using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
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
        UserResponse User,
        DateTime CreationDate,
        bool IsPublic,
        Uri? ImageUri
    );

    public record UserResponse(EntityId Id, string UserName, Uri? Avatar);

    public static async Task<Results<Ok<Response[]>, BadRequest>> Handle(
        string name,
        ApiDbContext dbContext,
        UserService userService
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return TypedResults.BadRequest();
        }

        var (user, isAdmin) = await userService.CurrentUser();

        var playlists = await dbContext
            .Playlists.Where(e => EF.Functions.ILike(e.Name, $"%{name}%"))
            .WhereIf(!isAdmin, e => e.IsPublic || e.UserId == user.Id)
            .OrderBy(e => e.Id)
            .Take(50)
            .Select(e => new Response(
                e.Name,
                e.Description,
                e.Id,
                new(e.UserId, e.User.UserName!, e.User.AvatarUri),
                e.CreationDate,
                e.IsPublic,
                e.ImageUri
            ))
            .ToArrayAsync();

        return TypedResults.Ok(playlists);
    }
}
