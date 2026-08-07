using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public sealed class Profile : Entity
{
    public Uri? AvatarUri { get; set; }
    public ICollection<Playlist> Playlists { get; } = null!;
    public Artist? Artist { get; set; }
    public PlaybackQueue Queue { get; set; }
    public AppUser User { get; set; }
    public EntityId UserId { get; set; }
}

public class ProfileConfig : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.HasMany(e => e.Playlists).WithOne(e => e.User);
        builder.HasOne(e => e.User).WithOne(e => e.Profile);
        builder.HasOne(e => e.Queue).WithOne(e => e.Owner);
    }
}
