using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Playlists;

public class GetEntries : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id}/tracks", Handle);

    public record Response(
        EntityId Id,
        DateTime CreationDate,
        int Index,
        TrackResponse Track
    );

    public record TrackResponse(
        string Title,
        string Genre,
        Uri? DataUri,
        long Duration,
        EntityId Id,
        DateTime CreationDate,
        AlbumResponse Album,
        ArtistResponse PrimaryArtist,
        ArtistResponse[] OtherArtists
    );

    public record ArtistResponse(EntityId Id, string Name);

    public record AlbumResponse(EntityId Id, string Title, Uri ImageUri);

    private static async Task<Results<Ok<Response[]>, BadRequest>> Handle(
        EntityId id,
        ApiDbContext dbContext,
        UserService userService
    )
    {
        var (user, isAdmin) = await userService.CurrentUser();

        var playlist = await dbContext.Playlists.AnyAsync(e =>
            e.Id == id && (e.UserId == user.Id || e.IsPublic)
        );

        if (playlist is false)
        {
            return TypedResults.BadRequest();
        }

        var tracks = await dbContext
            .PlaylistTracks.Where(e => e.PlaylistId == id)
            .WhereIf(
                !isAdmin,
                e => e.Playlist.UserId == user.Id || e.Playlist.IsPublic
            )
            .Select(e => new Response(
                e.Id,
                e.CreationDate,
                e.Index,
                new TrackResponse(
                    e.Track.Title,
                    e.Track.Genre,
                    e.Track.DataUri,
                    e.Track.Duration,
                    e.Track.Id,
                    e.Track.CreationDate,
                    new(
                        e.Track.AlbumId,
                        e.Track.Album.Title,
                        e.Track.Album.ImageUri
                    ),
                    new(e.Track.PrimaryArtistId, e.Track.PrimaryArtist.Name),
                    e.Track.Artists.Select(a => new ArtistResponse(
                            a.Id,
                            a.Name
                        ))
                        .ToArray()
                )
            ))
            .ToArrayAsync();

        return TypedResults.Ok(tracks);
    }
}
