using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Services.Api;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Features.Playlists;

public class DeletePlaylist : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/{id}", Handle);

    private static async Task<Results<NoContent, BadRequest>> Handle(
        EntityId id,
        ApiDbContext dbContext,
        UserService userService,
        HttpContext context
    )
    {
        var user = await userService.GetUser();

        var playlist = await dbContext
            .Playlists.Where(e => e.Id == id)
            .FirstOrDefaultAsync();

        if (
            playlist is null
            || playlist.UserId != user.Id
            || await userService.IsNotAdmin(user)
        )
        {
            return TypedResults.BadRequest();
        }

        dbContext.Playlists.Remove(playlist);
        await dbContext.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}
