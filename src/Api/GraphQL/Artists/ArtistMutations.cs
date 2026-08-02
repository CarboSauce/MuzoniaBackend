using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Artists;

[MutationType]
public static class ArtistMutations
{
    public static async Task<Artist> CreateArtist(
        CreateArtistInput input,
        ApiDbContext dbContext,
        ClaimsPrincipal user
    )
    {
        var userId = user.UserId;

        if (await dbContext.Artists.AnyAsync(a => a.Name == input.Name))
        {
            throw new GraphQLException("Artist with such name already exists");
        }
        if (await dbContext.Artists.AnyAsync(a => a.UserId == userId))
        {
            throw new GraphQLException("You already have an artist profile");
        }

        var artist = new Artist
        {
            UserId = userId,
            Name = input.Name,
            Description = input.Description,
            ImageUri = new Uri("file://dummy.pl"),
        };

        await dbContext.Artists.AddAsync(artist);
        await dbContext.SaveChangesAsync();

        return artist;
    }
}

public record CreateArtistInput(string Name, string Description);
