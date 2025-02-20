using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Album;

public class DeleteAlbum : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/{id}", Handle);

    public static async Task<Results<NoContent, BadRequest>> Handle(
        EntityId id,
        ApiDbContext dbContext,
        UserService userService
    )
    {
        var (user, isAdmin) = await userService.CurrentUser();

        var album = await dbContext
            .Albums.Where(a => a.Id == id)
            .WhereIf(!isAdmin, a => a.Owner.UserId == user.Id)
            .FirstOrDefaultAsync();

        if (album is null)
        {
            return TypedResults.BadRequest();
        }

        dbContext.Albums.Remove(album);
        await dbContext.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}
