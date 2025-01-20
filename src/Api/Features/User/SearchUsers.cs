using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;

namespace Muzonia.Api.Features.User;

public class SearchUsers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/search/{name}", Handle);

    public record Response(
        EntityId Id,
        DateTime CreationDate,
        string Name,
        Uri? AvatarUri
    );

    private static async Task<
        Results<Ok<Response[]>, BadRequest, ForbidHttpResult>
    > Handle(string name, HttpContext context, ApiDbContext dbContext)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return TypedResults.BadRequest();
        }

        if (!context.IsLoggedIn())
        {
            return TypedResults.Forbid();
        }

        var users = await dbContext
            .Users.Where(e =>
                e.UserName != null
                && EF.Functions.ILike(e.UserName, $"%{name}%")
            )
            .Select(e => new Response(
                e.Id,
                e.CreationDate,
                e.UserName!,
                e.AvatarUri
            ))
            .Take(50)
            .ToArrayAsync();

        return TypedResults.Ok(users);
    }
}
