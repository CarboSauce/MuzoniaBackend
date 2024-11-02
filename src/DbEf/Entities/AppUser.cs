using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class AppUser : IdentityUser<Ulid>
{
    public required Uri? AvatarUri { get; set; }
    public required DateTime CreationDate { get; set; }

    public virtual ICollection<Playlist> Playlists { get; } = null!;
    public virtual Artist? Artist { get; set; }
}

public class AppUserConfig : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasOne(e => e.Artist).WithOne(e => e.User);
        builder.HasMany(e => e.Playlists).WithOne(e => e.User);
    }
}
