using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Query;

public class ArtistDto
{
    public required EntityId Id { get; set; }
    public required string Name { get; set; } = null!;
    public required string? Description { get; set; }
    public required Uri? ImageUri { get; set; }
    public required DateTime CreationDate { get; set; }
}

public partial class Query
{
    [UseFirstOrDefault]
    [UseProjection]
    [Authorize]
    public IQueryable<ArtistDto> GetArtistById(
        EntityId id,
        [Service] ApiDbContext dbContext
    )
    {
        return dbContext
            .Artists.Where(a => a.Id == id)
            .Select(a => new ArtistDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                ImageUri = a.ImageUri,
                CreationDate = a.CreationDate,
            });
    }
}

[ExtendObjectType(typeof(object))]
public class ArtistResolvers : ITypeResolver
{
    [UseFirstOrDefault]
    [UseProjection]
    [Authorize]
    public IQueryable<ArtistDto> GetArtist(
        [Parent] UserDto user,
        [Service] ApiDbContext dbContext
    )
    {
        return dbContext
            .Artists.Where(a => a.UserId == user.Id)
            .Select(a => new ArtistDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                ImageUri = a.ImageUri,
                CreationDate = a.CreationDate,
            });
    }

    [UseFirstOrDefault]
    [UseProjection]
    [Authorize]
    public IQueryable<UserDto> GetUser(
        [Parent] ArtistDto artist,
        [Service] ApiDbContext dbContext,
        HttpContext context
    )
    {
        var userId = context.GetUserId();

        return dbContext
            .Artists.Where(a => a.Id == artist.Id)
            .Include(a => a.User)
            .Select(a => a.User)
            .Select(a => new UserDto
            {
                Id = a.Id,
                Username = a.UserName!,
                AvatarUri = a.AvatarUri,
                CreationDate = a.CreationDate,
                Email = a.Id == userId ? a.Email : null,
            });
    }
}
