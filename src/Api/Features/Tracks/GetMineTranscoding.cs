using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Tracks;

public class GetMineTranscoding : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/transcoding", Handle);

    public record Response(
        string Title,
        string Genre,
        EntityId Id,
        DateTime CreationDate,
        AlbumResponse Album,
        ArtistResponse PrimaryArtist,
        ArtistResponse[] OtherArtists
    );

    public record AlbumResponse(EntityId Id, string Title);

    public record ArtistResponse(EntityId Id, string Name);

    private static async Task<Results<Ok<Response[]>, BadRequest>> Handle(
        EntityId id,
        ApiDbContext dbContext,
        ArtistService artistService,
        UserService userService
    )
    {
        var (user, isAdmin) = await userService.CurrentUser();
        var artist = await artistService.GetArtist(user);

        if (artist is null)
        {
            return TypedResults.BadRequest();
        }

        var track = await dbContext
            .Tracks.Where(e => e.PrimaryArtistId == artist.Id)
            .Select(e => new Response(
                e.Title,
                e.Genre,
                e.Id,
                e.CreationDate,
                new(e.AlbumId, e.Album.Title),
                new(e.PrimaryArtistId, e.PrimaryArtist.Name),
                e.Artists.Select(a => new ArtistResponse(a.Id, a.Name))
                    .ToArray()
            ))
            .ToArrayAsync();

        return TypedResults.Ok(track);
    }
}
