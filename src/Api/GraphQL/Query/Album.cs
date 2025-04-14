using HotChocolate.Authorization;

namespace Muzonia.Api.GraphQL.Query;

public class AlbumDto
{
    public required EntityId Id { get; set; }
    public required EntityId OwnerId { get; set; }
    public required string Title { get; set; }
    public required Uri ImageUri { get; set; }
    public required DateTime CreationDate { get; set; }
}

public partial class Query
{
    [UseFirstOrDefault]
    [UseProjection]
    [Authorize]
    public IQueryable<AlbumDto> GetAlbumById(
        EntityId id,
        [Service] ApiDbContext dbContext
    )
    {
        return dbContext
            .Albums.Where(a => a.Id == id)
            .Select(a => new AlbumDto
            {
                Id = a.Id,
                OwnerId = a.OwnerId,
                Title = a.Title,
                ImageUri = a.ImageUri,
                CreationDate = a.CreationDate
            });
    }
}

[ExtendObjectType(typeof(object))]
public class AlbumResolvers : ITypeResolver
{
    [UsePaging]
    [UseProjection]
    [UseSorting]
    [Authorize]
    public IQueryable<AlbumDto> GetAlbums(
        [Parent] ArtistDto artist,
        [Service] ApiDbContext dbContext
    )
    {
        return dbContext
            .ArtistAlbums.Where(a => a.ArtistId == artist.Id)
            .Select(a => a.Album)
            .Select(a => new AlbumDto
            {
                Id = a.Id,
                OwnerId = a.OwnerId,
                Title = a.Title,
                ImageUri = a.ImageUri,
                CreationDate = a.CreationDate,
            });
    }

    [UseProjection]
    [Authorize]
    public IQueryable<ArtistDto> GetArtists(
        [Parent] AlbumDto album,
        [Service] ApiDbContext dbContext
    )
    {
        return dbContext
            .ArtistAlbums.Where(a => a.AlbumId == album.Id)
            .Select(a => a.Artist)
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
