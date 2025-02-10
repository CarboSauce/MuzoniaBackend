using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public sealed class AppUser : IdentityUser<EntityId>
{
    public Uri? AvatarUri { get; set; }
    public DateTime CreationDate { get; set; }

    public ICollection<Playlist> Playlists { get; } = null!;
    public Artist? Artist { get; set; }
    public PlaybackQueue Queue { get; set; }

    public AppUser()
    {
        CreationDate = DateTime.UtcNow;
        Id = EntityId.NewGuid();
        SecurityStamp = EntityId.NewGuid().ToString();
    }
}

public class AppUserConfig : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasOne(e => e.Artist).WithOne(e => e.User);
        builder.HasMany(e => e.Playlists).WithOne(e => e.User);
        builder.HasOne(e => e.Queue).WithOne(e => e.Owner);
    }
}
