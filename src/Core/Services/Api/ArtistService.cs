using System.Net;
using System.Security.Claims;
using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Muzonia.Core.Common;
using Muzonia.Core.Dto.Request;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Exceptions;
using Muzonia.Core.Utils;
using Muzonia.DbEf;
using Muzonia.DbEf.Entities;

namespace Muzonia.Core.Services.Api;

public class ArtistService(
    ClaimsPrincipal claims,
    UserManager<AppUser> userManager,
    ApiDbContext dbContext,
    IFileWriter fileWriter
) : ITransient
{
    public async Task<ArtistResponse> CreateArtist(
        EntityId userId,
        CreateArtistRequest request
    )
    {
        var user = await userManager.GetUserAsync(claims);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        await EnsureUniqueName(request);

        var targetUser = await userManager.FindByIdAsync(userId.ToString());

        if (targetUser is null)
        {
            throw new NotFoundException("Target user not found");
        }

        await EnsureSingleArtist(targetUser);

        var fileUri = await fileWriter.WriteAsync(
            request.File,
            "images/",
            Guid.NewGuid().ToString()
        );

        if (fileUri is null)
        {
            throw new BadRequestException("Failed to upload image");
        }

        var artist = new Artist
        {
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            ImageUri = fileUri,
        };

        await dbContext.Artists.AddAsync(artist);
        await dbContext.SaveChangesAsync();

        return new(artist);
    }

    public async Task<ArtistResponse?> GetMyArtist()
    {
        var user = await userManager.GetCurrentUser(claims);

        var artists = await dbContext
            .Artists.Where(a => a.UserId == user.Id)
            .Select(a => new ArtistResponse(a))
            .SingleOrDefaultAsync();

        return artists;
    }

    public async Task<ArtistResponse?> GetArtist(AppUser user) =>
        await dbContext
            .Artists.Where(a => a.UserId == user.Id)
            .Select(a => new ArtistResponse(a))
            .SingleOrDefaultAsync();

    public async Task<ArtistResponse> UpdateArtist(
        Guid id,
        UpdateArtistRequest request
    )
    {
        var artist = await dbContext.Artists.FindAsync(id);

        if (artist is null)
        {
            throw new NotFoundException("Artist not found");
        }

        if (request.Name is not null)
            artist.Name = request.Name;

        if (request.Description is not null)
            artist.Description = request.Description;

        if (request.File is not null)
        {
            var file = await fileWriter.WriteAsync(
                request.File,
                "images/",
                Guid.NewGuid().ToString()
            );
            if (file is not null)
            {
                artist.ImageUri = file;
            }
        }

        await dbContext.SaveChangesAsync();

        return new(artist);
    }

    internal async Task<bool> IsUserArtist(Guid userId, Guid[] artistId) =>
        await dbContext
            .Artists.Where(a => a.UserId == userId)
            .AnyAsync(a => artistId.Contains(a.Id));

    private async Task EnsureUniqueName(CreateArtistRequest request)
    {
        if (await dbContext.Artists.AnyAsync(a => a.Name == request.Name))
        {
            throw new BadRequestException("Artist already exists");
        }
    }

    private async Task EnsureSingleArtist(AppUser user)
    {
        if (await dbContext.Artists.AnyAsync(a => a.UserId == user.Id))
        {
            throw new BadRequestException("You already have an artist");
        }
    }
}
