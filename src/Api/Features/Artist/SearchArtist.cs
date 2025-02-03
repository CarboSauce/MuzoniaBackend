using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Dto.Response;

namespace Muzonia.Api.Features.Artist;

public class SearchArtist : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/search/{name}", Handle);

    public class Validator : AbstractValidator<string>
    {
        public Validator()
        {
            RuleFor(x => x).NotEmpty().Length(1, 128);
        }
    }

    private static async Task<
        Results<Ok<ArtistResponse[]>, ValidationProblem, UnauthorizedHttpResult>
    > Handle(
        string name,
        ApiDbContext dbContext,
        HttpContext context,
        Validator validator
    )
    {
        if (!context.IsLoggedIn())
        {
            return TypedResults.Unauthorized();
        }

        var validationResult = validator.Validate(name);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(
                validationResult.ToDictionary()
            );
        }

        var artists = await dbContext
            .Artists.Where(a =>
                EF.Functions.ILike(a.Name, $"%{name}%")
                || EF.Functions.ILike(a.Description, $"%{name}%")
            )
            .Select(a => new ArtistResponse(
                a.Id,
                a.UserId,
                a.Name,
                a.Description,
                a.ImageUri,
                a.CreationDate
            ))
            .Take(50)
            .OrderBy(a => a.Id)
            .ToArrayAsync();

        return TypedResults.Ok(artists);
    }
}
