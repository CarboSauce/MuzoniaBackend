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

    public static async Task<Results<NoContent, BadRequest>> Handle(
        EntityId id,
        ApiDbContext dbContext,
        UserService userService
    )
    {
        var (user, isAdmin) = await userService.CurrentUser();

        var playlist = await dbContext
            .Playlists.Where(e => e.Id == id)
            .WhereIf(!isAdmin, e => e.UserId == user.Id)
            .FirstOrDefaultAsync();

        if (playlist is null)
        {
            return TypedResults.BadRequest();
        }

        dbContext.Playlists.Remove(playlist);
        await dbContext.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}
