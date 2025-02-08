using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Playback;

public class GetDevices : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/devices", Handle);

    public record Response(EntityId Id, string Name);

    private static async Task<Results<Ok<Response[]>, BadRequest>> Handle(
        ApiDbContext dbContext,
        UserService userService
    )
    {
        var user = await userService.GetUser();

        var result = await dbContext
            .Devices.Where(e => e.UserId == user.Id)
            .Select(e => new Response(e.Id, e.Name))
            .ToArrayAsync();

        return TypedResults.Ok(result);
    }
}
