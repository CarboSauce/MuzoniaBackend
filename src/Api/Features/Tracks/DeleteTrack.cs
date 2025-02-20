using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Tracks;

public class DeleteTrack : IEndpoint
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

        var track = await dbContext
            .Tracks.Where(t => t.Id == id)
            .WhereIf(!isAdmin, t => t.PrimaryArtist.UserId == user.Id)
            .FirstOrDefaultAsync();

        if (track is null)
        {
            return TypedResults.BadRequest();
        }

        dbContext.Tracks.Remove(track);
        await dbContext.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}
