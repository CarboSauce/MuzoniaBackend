using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Album;

public class SearchAlbum : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/search/{name}", Handle);

    public record ArtistResponse(EntityId Id, string Name);

    public record Response(
        EntityId Id,
        string Title,
        Uri ImageUri,
        ArtistResponse[] Artists
    );

    private static async Task<
        Results<Ok<Response[]>, BadRequest, ForbidHttpResult>
    > Handle(string name, ApiDbContext dbContext, HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return TypedResults.BadRequest();
        }

        if (!context.IsLoggedIn())
        {
            return TypedResults.Forbid();
        }

        var albums = await dbContext
            .Albums.Where(e =>
                EF.Functions.ILike(e.Title, $"%{name}%")
                || EF.Functions.ILike(e.Owner.Name, $"%{name}%")
            )
            .Take(50)
            .OrderBy(a => a.Id)
            .Select(e => new Response(
                e.Id,
                e.Title,
                e.ImageUri,
                e.Artists.Select(a => new ArtistResponse(a.Id, a.Name))
                    .ToArray()
            ))
            .ToArrayAsync();

        return TypedResults.Ok(albums);
    }
}
