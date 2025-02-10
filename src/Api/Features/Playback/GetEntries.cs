using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Playback;

public class GetEntries : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{queueId}/entries", Handle);

    public record Response(EntityId Id, int Index, TrackResponse Track);

    public record TrackResponse(
        EntityId Id,
        string Title,
        Uri? DataUri,
        long Duration,
        DateTime CreationDate,
        AlbumReponse Album,
        ArtistResponse PrimaryArtist,
        ArtistResponse[] OtherArtists
    );

    public record AlbumReponse(EntityId Id, string Title, Uri ImageUri);

    public record ArtistResponse(EntityId Id, string Name);

    private static async Task<Results<Ok<Response[]>, BadRequest>> Handle(
        EntityId queueId,
        UserService userService,
        ApiDbContext dbContext
    )
    {
        var user = await userService.GetUser();

        var result = await dbContext
            .QueueEntries.Where(e =>
                e.QueueId == queueId
                && (
                    e.Queue.OwnerId == user.Id
                    || e.Queue.QueueUsers.Any(u => u.UserId == user.Id)
                )
            )
            .OrderBy(e => e.Index)
            .Select(e => new Response(
                e.Id,
                e.Index,
                new TrackResponse(
                    e.Track.Id,
                    e.Track.Title,
                    e.Track.DataUri,
                    e.Track.Duration,
                    e.Track.CreationDate,
                    new AlbumReponse(
                        e.Track.Album.Id,
                        e.Track.Album.Title,
                        e.Track.Album.ImageUri
                    ),
                    new ArtistResponse(
                        e.Track.PrimaryArtist.Id,
                        e.Track.PrimaryArtist.Name
                    ),
                    e.Track.Artists.Select(a => new ArtistResponse(
                            a.Id,
                            a.Name
                        ))
                        .ToArray()
                )
            ))
            .ToArrayAsync();

        return TypedResults.Ok(result);
    }
}
