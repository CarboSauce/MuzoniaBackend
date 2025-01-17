using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Muzonia.Api.Features.Artist;

public class GetArtist : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id}", Handle);

    public record UserResponse(EntityId Id, string? Username);

    public record GetAristResponse(
        EntityId Id,
        UserResponse User,
        string Name,
        string Description,
        Uri ImageUri,
        DateTime CreationDate
    );

    private static async Task<Results<Ok<GetAristResponse>, NotFound>> Handle(
        ApiDbContext dbContext,
        EntityId Id
    )
    {
        var artist = await dbContext
            .Artists.Where(a => a.Id == Id)
            .Select(a => new GetAristResponse(
                a.Id,
                new(a.UserId, a.User.UserName),
                a.Name,
                a.Description,
                a.ImageUri,
                a.CreationDate
            ))
            .FirstOrDefaultAsync();

        return artist switch
        {
            null => TypedResults.NotFound(),
            _ => TypedResults.Ok(artist),
        };
    }
}
