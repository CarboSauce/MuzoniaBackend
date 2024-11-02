using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class Album
{
    public Ulid Id { get; set; } = Ulid.NewUlid();

    public Ulid SongsId { get; set; }
    public virtual ICollection<Song> Songs { get; } = null!;
    public required string Title { get; set; }
    public required Uri ImageUri { get; set; }
    public virtual ICollection<Artist> Artists { get; } = null!;
}

public class AlbumConfig : IEntityTypeConfiguration<Album>
{
    public void Configure(EntityTypeBuilder<Album> builder)
    {
        builder.HasKey(e => e.Id);
        builder
            .HasMany(e => e.Songs)
            .WithOne(e => e.Album)
            .HasForeignKey(e => e.AlbumId)
            .IsRequired();
        builder.Property(e => e.Title).IsRequired();
        builder.Property(e => e.ImageUri).IsRequired();
    }
}
