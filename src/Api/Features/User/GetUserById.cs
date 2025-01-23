using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.User;

public class GetUserById : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id}", Handle);

    public record Response(
        EntityId Id,
        string UserName,
        DateTime CreationDate,
        Uri? Avatar,
        EntityId? ArtistId,
        bool IsAdmin
    );

    private static async Task<
        Results<Ok<Response>, ForbidHttpResult, NotFound>
    > Handle(EntityId id, HttpContext context, ApiDbContext dbContext)
    {
        if (!context.IsLoggedIn())
        {
            return TypedResults.Forbid();
        }

        var user = await dbContext
            .Users.Where(u => u.Id == id)
            .Select(u => new Response(
                u.Id,
                u.UserName!,
                u.CreationDate,
                u.AvatarUri,
                u.Artist != null ? u.Artist.Id : null,
                dbContext.UserRoles.Any(ur =>
                    ur.RoleId == ApiDbContext.AdminRoleId && ur.UserId == id
                )
            ))
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(user);
    }
}
