using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class Album
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public virtual ICollection<Track> Tracks { get; } = null!;
    public required string Title { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public required Uri ImageUri { get; set; }
    public virtual ICollection<Artist> Artists { get; } = null!;
}

public class AlbumConfig : IEntityTypeConfiguration<Album>
{
    public void Configure(EntityTypeBuilder<Album> builder)
    {
        builder.HasKey(e => e.Id);
        builder
            .HasMany(e => e.Tracks)
            .WithOne(e => e.Album)
            .HasForeignKey(e => e.AlbumId)
            .IsRequired();

        builder
            .HasMany(e => e.Artists)
            .WithMany(e => e.Albums)
            .UsingEntity<ArtistAlbum>();
        builder.Property(e => e.Title).HasMaxLength(256);
        builder.Property(e => e.ImageUri).IsRequired();
    }
}
