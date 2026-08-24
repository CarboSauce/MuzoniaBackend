using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public sealed class AppUser : Entity
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public string? ImageUri { get; set; }
    public PlaybackQueue? Queue { get; set; }
    public Artist? Artist { get; set; }
    public ICollection<Playlist> Playlists { get; } = null!;
}

public class AppUserConfig : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(e => e.Email).HasColumnName("email");
        builder.Property(e => e.Name).HasColumnName("name");
        builder.Property(e => e.Role).HasColumnName("role");
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.ImageUri).HasColumnName("image");
        builder.Property(e => e.CreationDate).HasColumnName("created_at");

        builder
            .HasIndex(u => new { u.Name })
            .HasMethod("GIN")
            .IsTsVectorExpressionIndex("english");
    }
}
