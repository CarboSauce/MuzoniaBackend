using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class Album : Entity
{
    public virtual ICollection<Track>? Tracks { get; }
    public required string Title { get; set; }
    public required EntityId OwnerId { get; set; }
    public Artist Owner { get; } = null!;
    public required Uri? ImageUri { get; set; }
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
            .IsRequired(false);

        builder.HasOne(e => e.Owner).WithMany(e => e.OwnedAlbums);

        builder
            .HasMany(e => e.Artists)
            .WithMany(e => e.Albums)
            .UsingEntity<ArtistAlbum>();
        builder.Property(e => e.Title).HasMaxLength(256);
        builder.Property(e => e.ImageUri).IsRequired();

        builder
            .HasIndex(a => new { a.Title })
            .HasMethod("GIN")
            .IsTsVectorExpressionIndex("english");
    }
}
