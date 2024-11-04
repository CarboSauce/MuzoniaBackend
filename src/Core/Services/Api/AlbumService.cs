using System.Security.Claims;
using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muzonia.Core.Common;
using Muzonia.Core.Dto.Request;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Exceptions;
using Muzonia.DbEf;
using Muzonia.DbEf.Entities;

namespace Muzonia.Core.Services.Api;

public class AlbumService(
    UserManager<AppUser> userManager,
    ApiDbContext dbContext,
    ArtistService artistService,
    IFileWriter fileWriter,
    ClaimsPrincipal claims,
    ILogger<AlbumService> logger
) : ITransient
{
    public async Task<AlbumResponse> CreateAlbum(CreateAlbumRequest request)
    {
        var user = await userManager.GetCurrentUser(claims);

        if (!await artistService.IsUserArtist(user.Id, request.ArtistIds))
        {
            throw new ForbiddenException(
                "You are not allowed to create album for this artist"
            );
        }

        var file = await fileWriter.WriteAsync(request.File, "images/");

        if (file is null)
        {
            throw new BadRequestException("Failed to upload image");
        }

        var artistsCount = await dbContext.Artists.CountAsync(a =>
            request.ArtistIds.Contains(a.Id)
        );

        if (artistsCount != request.ArtistIds.Length)
        {
            throw new NotFoundException("Artist not found");
        }

        var album = new Album { Title = request.Name, ImageUri = file, };

        dbContext.Albums.Add(album);

        foreach (var artistId in request.ArtistIds)
        {
            dbContext.ArtistAlbums.Add(
                new ArtistAlbum { ArtistId = artistId, AlbumId = album.Id, }
            );
        }

        await dbContext.SaveChangesAsync();

        var artists = await dbContext
            .Artists.Where(a => request.ArtistIds.Contains(a.Id))
            .Select(a => new ArtistResponse(a))
            .ToArrayAsync();

        return new(album, artists);
    }

    public async Task<IEnumerable<AlbumResponse>> GetMyAlbums()
    {
        var user = await userManager.GetCurrentUser(claims);

        var albums = await dbContext
            .Albums.Where(a => a.Artists.Any(b => b.UserId == user.Id))
            .Select(a => new AlbumResponse(
                a,
                a.Artists.Select(b => new ArtistResponse(b)).ToArray()
            ))
            .ToListAsync();

        return albums;
    }

    public async Task<IEnumerable<AlbumResponse>> GetArtistAlbums(
        Guid artistId
    ) =>
        await dbContext
            .Albums.Where(a => a.Artists.Any(b => b.Id == artistId))
            .Select(a => new AlbumResponse(
                a,
                a.Artists.Select(b => new ArtistResponse(b)).ToArray()
            ))
            .ToListAsync();

    public async Task<IEnumerable<AlbumResponse>> GetArtistAlbumsPaginate(
        Guid artistId,
        Guid pageId,
        int limit
    )
    {
        logger.LogDebug("Getting albums for artist {artistId}", artistId);
        return await dbContext
            .Albums.Where(a => a.Artists.Any(b => b.Id == artistId))
            .OrderBy(a => a.CreationDate)
            .Where(a => a.Id.ToString().CompareTo(pageId.ToString()) > 0)
            .Take(limit)
            .Select(a => new AlbumResponse(
                a,
                a.Artists.Select(b => new ArtistResponse(b)).ToArray()
            ))
            .ToListAsync();
    }
}
