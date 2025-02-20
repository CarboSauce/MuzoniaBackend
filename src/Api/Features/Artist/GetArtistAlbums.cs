using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Dto.Response;

namespace Muzonia.Api.Features.Artist;

public class GetArtistAlbums : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id}/albums", Handle);

    public static async Task<
        Results<Ok<AlbumResponse[]>, UnauthorizedHttpResult>
    > Handle(EntityId id, ApiDbContext dbContext, HttpContext context)
    {
        if (!context.IsLoggedIn())
        {
            return TypedResults.Unauthorized();
        }

        var tracks = await dbContext
            .Albums.Where(a => a.Artists.Any(b => b.Id == id))
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
            .ToArrayAsync();

        return TypedResults.Ok(tracks);
    }
}
