using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class Track
{
    public EntityId Id { get; set; } = NewId.Create();
    public required Uri? DataUri { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public required string Title { get; set; }
    public required string Genre { get; set; }
    public long Duration { get; set; }
    public required EntityId? AlbumId { get; set; }
    public Album? Album { get; set; }
    public required EntityId PrimaryArtistId { get; set; }
    public Artist PrimaryArtist { get; set; } = null!;
    public virtual ICollection<Artist> Artists { get; } = null!;
    public virtual ICollection<Playlist> Playlists { get; } = null!;
}

public class SongConfig : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasMaxLength(128);
        builder.Property(e => e.Genre).HasMaxLength(128);
        builder
            .HasOne(e => e.PrimaryArtist)
            .WithMany(e => e.PrimaryTracks)
            .HasForeignKey(e => e.PrimaryArtistId);

        builder
            .HasMany(e => e.Artists)
            .WithMany(e => e.Tracks)
            .UsingEntity<TrackArtist>();

        builder
            .HasMany(e => e.Playlists)
            .WithMany(e => e.Tracks)
            .UsingEntity<PlaylistTrack>();

        builder
            .HasOne(e => e.Album)
            .WithMany(e => e.Tracks)
            .HasForeignKey(e => e.AlbumId)
            .IsRequired(false);

        builder.Property(e => e.Title).IsRequired();
        builder.Property(e => e.CreationDate).IsRequired();

        builder
            .HasIndex(b => b.Title)
            .HasMethod("GIN")
            .IsTsVectorExpressionIndex("english");
    }
}
