using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Artist;

public class DeleteArtist : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/{id}", Handle);

    private static async Task<Results<NoContent, NotFound>> Handle(
        EntityId id,
        ApiDbContext dbContext,
        UserService userService,
        CancellationToken token
    )
    {
        var (user, isAdmin) = await userService.CurrentUser();

        var result = await dbContext
            .Artists.Where(a => a.Id == id)
            .WhereIf(!isAdmin, a => a.UserId == user.Id)
            .ExecuteDeleteAsync(token);

        return result is 0 ? TypedResults.NotFound() : TypedResults.NoContent();
    }
}
