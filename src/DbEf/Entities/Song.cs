using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class Song
{
    public Ulid Id { get; set; } = Ulid.NewUlid();
    public required Uri DataUri { get; set; }
    public required DateTime CreationDate { get; set; }
    public required string Title { get; set; }
    public Ulid AlbumId { get; set; }
    public required Album Album { get; set; }
    public Ulid PrimaryArtistId { get; set; }
    public required Artist PrimaryArtist { get; set; }
    public virtual ICollection<Artist> Artists { get; } = null!;
    public virtual ICollection<Playlist> Playlists { get; } = null!;
}

public class SongConfig : IEntityTypeConfiguration<Song>
{
    public void Configure(EntityTypeBuilder<Song> builder)
    {
        builder.HasKey(e => e.Id);
        builder
            .HasOne(e => e.PrimaryArtist)
            .WithMany(e => e.PrimarySongs)
            .HasForeignKey(e => e.PrimaryArtistId);
        builder
            .HasMany(e => e.Artists)
            .WithMany(e => e.Songs)
            .UsingEntity<SongArtist>();

        builder
            .HasMany(e => e.Playlists)
            .WithMany(e => e.Songs)
            .UsingEntity<PlaylistSong>();

        builder
            .HasOne(e => e.Album)
            .WithMany(e => e.Songs)
            .HasForeignKey(e => e.AlbumId);

        builder.Property(e => e.DataUri).IsRequired();
        builder.Property(e => e.Title).IsRequired();
        builder.Property(e => e.CreationDate).IsRequired();
    }
}
