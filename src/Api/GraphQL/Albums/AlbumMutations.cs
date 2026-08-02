using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Core.Exceptions;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Albums;

[MutationType]
public static class AlbumMutations
{
    public static async Task<Album> CreateAlbum(
        CreateAlbumInput input,
        ApiDbContext dbContext,
        ClaimsPrincipal claims
    )
    {
        var userId = claims.UserId;

        var artist = await dbContext
            .Artists.Where(a => a.UserId == userId)
            .FirstOrDefaultAsync();

        if (artist is null)
        {
            throw new GraphQLException("User is not an artist");
        }

        if (await dbContext.Albums.AnyAsync(a => a.Title == input.Name))
        {
            throw new GraphQLException("Album already exists");
        }

        var album = new Album
        {
            OwnerId = artist.Id,
            Title = input.Name,
            ImageUri = new("file://nofile"),
        };

        dbContext.Albums.Add(album);

        dbContext.ArtistAlbums.Add(
            new ArtistAlbum { ArtistId = artist.Id, AlbumId = album.Id }
        );

        await dbContext.SaveChangesAsync();

        return album;
    }
}

public sealed record CreateAlbumInput(string Name);
