using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Tracks;

public class UpdateTrack : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{id}", Handle).WithValidation<Validator>();

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Title).MaximumLength(128);
            RuleFor(x => x.Genre).MaximumLength(128);
        }
    }

    public record Request(string? Title, string? Genre, EntityId? AlbumId);

    public record Response(
        EntityId Id,
        string Title,
        string Genre,
        DateTime CreationDate,
        EntityId AlbumId,
        EntityId PrimaryArtistId
    );

    private static async Task<Results<Ok<Response>, BadRequest<string>>> Handle(
        EntityId id,
        Request req,
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
            return TypedResults.BadRequest($"No track with id {id}");
        }

        track.Title = req.Title ?? track.Title;
        track.Genre = req.Genre ?? track.Genre;
        track.AlbumId = req.AlbumId ?? track.AlbumId;

        await dbContext.SaveChangesAsync();

        return TypedResults.Ok(
            new Response(
                track.Id,
                track.Title,
                track.Genre,
                track.CreationDate,
                track.AlbumId,
                track.PrimaryArtistId
            )
        );
    }
}
