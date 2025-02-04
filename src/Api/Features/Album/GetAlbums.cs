using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Album;

public class GetAlbums : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id}", Handle).WithDescription("Get album by id");

    private static async Task<
        Results<Ok<AlbumResponse>, UnauthorizedHttpResult>
    > Handle(EntityId id, ApiDbContext dbContext, HttpContext context)
    {
        if (!context.IsLoggedIn())
        {
            return TypedResults.Unauthorized();
        }

        var tracks = await dbContext
            .Albums.Where(a => a.Id == id)
            .Select(a => new AlbumResponse(
                a.Id,
                a.OwnerId,
                a.Artists.Select(b => new ArtistResponse(
                        b.Id,
                        b.UserId,
                        b.Name,
                        b.Description,
                        b.ImageUri,
                        b.CreationDate
                    ))
                    .ToArray(),
                a.Title,
                a.ImageUri,
                a.CreationDate
            ))
            .FirstOrDefaultAsync();

        return TypedResults.Ok(tracks);
    }
}
