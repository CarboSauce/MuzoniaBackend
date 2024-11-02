using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class Artist
{
    public required Ulid Id { get; set; } = Ulid.NewUlid();
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required Uri ImageUri { get; set; }
    public required DateTime CreationDate { get; set; }
    public required Ulid UserId { get; set; }
    public required AppUser User { get; set; }
    public virtual ICollection<Song> PrimarySongs { get; } = null!;
    public virtual ICollection<Song> Songs { get; } = null!;
    public virtual ICollection<Album> Albums { get; } = null!;
}

public class ArtistConfig : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.User).WithOne(e => e.Artist);
        builder
            .HasMany(e => e.Songs)
            .WithMany(e => e.Artists)
            .UsingEntity<SongArtist>();
        builder
            .HasMany(e => e.Albums)
            .WithMany(e => e.Artists)
            .UsingEntity<ArtistAlbum>();
    }
}
