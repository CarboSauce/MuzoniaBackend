using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.Album;

public class EditAlbum : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPatch("/{id}", Handle).WithValidation<Request>();

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Title).MaximumLength(128);
        }
    }

    public record Request(string? Title, IFormFile? File);

    public record Response(
        EntityId Id,
        EntityId OwnerId,
        string Title,
        Uri ImageUri
    );

    private static async Task<Results<Ok<Response>, BadRequest>> Handle(
        EntityId id,
        [FromForm] Request req,
        ApiDbContext dbContext,
        UserService userService,
        IFileWriter fileWriter
    )
    {
        var (user, isAdmin) = await userService.CurrentUser();

        var album = await dbContext
            .Albums.Where(a => a.Id == id)
            .WhereIf(!isAdmin, a => a.Owner.UserId == user.Id)
            .FirstOrDefaultAsync();

        if (album is null)
        {
            return TypedResults.BadRequest();
        }

        if (req.File is not null)
        {
            var file = await fileWriter.WriteImage(req.File);

            album.ImageUri = file ?? album.ImageUri;
        }

        album.Title = req.Title ?? album.Title;

        await dbContext.SaveChangesAsync();

        return TypedResults.Ok(
            new Response(album.Id, album.OwnerId, album.Title, album.ImageUri)
        );
    }
}
