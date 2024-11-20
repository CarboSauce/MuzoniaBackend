using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class Playlist : Entity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required bool IsPublic { get; set; }
    public required EntityId UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public virtual ICollection<Track> Tracks { get; } = null!;
}

public class PlaylistConfig : IEntityTypeConfiguration<Playlist>
{
    public void Configure(EntityTypeBuilder<Playlist> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.User).WithMany();
        builder.Property(e => e.Name).HasMaxLength(256);
        builder.Property(e => e.Description).HasMaxLength(256);
        builder
            .HasMany(e => e.Tracks)
            .WithMany(e => e.Playlists)
            .UsingEntity<PlaylistTrack>(
                j =>
                    j.HasOne(ps => ps.Track)
                        .WithMany()
                        .HasForeignKey(ps => ps.SongId),
                j =>
                    j.HasOne(ps => ps.Playlist)
                        .WithMany()
                        .HasForeignKey(ps => ps.PlaylistId)
            );
    }
}
