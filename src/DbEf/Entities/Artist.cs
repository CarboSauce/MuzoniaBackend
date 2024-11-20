using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class Artist : Entity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required Uri ImageUri { get; set; }
    public required EntityId UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public virtual ICollection<Track> PrimarySongs { get; } = null!;
    public virtual ICollection<Track> Songs { get; } = null!;
    public virtual ICollection<Album> Albums { get; } = null!;
}

public class ArtistConfig : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).HasMaxLength(256);
        builder.HasIndex(e => new { e.Name, e.UserId }).IsUnique();
        builder.Property(e => e.Description).HasMaxLength(512);
        builder.HasOne(e => e.User).WithOne(e => e.Artist);
        builder
            .HasMany(e => e.Songs)
            .WithMany(e => e.Artists)
            .UsingEntity<TrackArtist>();
        builder
            .HasMany(e => e.Albums)
            .WithMany(e => e.Artists)
            .UsingEntity<ArtistAlbum>();
    }
}
