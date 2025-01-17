using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Muzonia.Core.Common;
using Muzonia.DbEf;
using Muzonia.DbEf.Entities;

namespace Muzonia.Core.Services;

public class DbSeed(
    UserManager<AppUser> userManager,
    ApiDbContext dbContext,
    AdminConfig adminConfig
)
{
    public async Task SeedAdminAsync()
    {
        var admin = new AppUser
        {
            UserName = adminConfig.UserName,
            Email = adminConfig.Email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, adminConfig.Password);

        var role = await userManager.GetRolesAsync(admin);
        if (role.Count == 0)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        await userManager.UpdateAsync(admin);
    }

    public async Task SeedBasicDataAsync()
    {
        var hasDummyUser = await dbContext.Artists.AnyAsync(a =>
            a.Name == "Dummy user"
        );
        if (hasDummyUser)
        {
            return;
        }

        var user = new AppUser
        {
            UserName = "muzonia",
            Email = "muzonia@muzonia.muzonia",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, "muzonia");

        var artist = new Artist
        {
            Name = "Dummy user",
            Description = "This is dummy user only for dev",
            ImageUri = new Uri("https://picsum.photos/200"),
            UserId = user.Id,
        };

        dbContext.Artists.Add(artist);

        var album = new Album
        {
            Title = "Dummy album",
            ImageUri = new Uri("https://picsum.photos/200"),
        };

        dbContext.Albums.Add(album);

        dbContext.ArtistAlbums.Add(
            new ArtistAlbum { ArtistId = artist.Id, AlbumId = album.Id, }
        );

        await dbContext.SaveChangesAsync();
    }
}
