using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class Playlist
{
    public Ulid Id { get; set; } = Ulid.NewUlid();
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required DateTime CreationDate { get; set; }
    public required Ulid UserId { get; set; }
    public required AppUser User { get; set; }
    public virtual ICollection<Song> Songs { get; } = null!;
}

public class PlaylistConfig : IEntityTypeConfiguration<Playlist>
{
    public void Configure(EntityTypeBuilder<Playlist> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.User).WithMany();
        builder
            .HasMany(e => e.Songs)
            .WithMany(e => e.Playlists)
            .UsingEntity<PlaylistSong>(
                j =>
                    j.HasOne(ps => ps.Song)
                        .WithMany()
                        .HasForeignKey(ps => ps.SongId),
                j =>
                    j.HasOne(ps => ps.Playlist)
                        .WithMany()
                        .HasForeignKey(ps => ps.PlaylistId)
            );
    }
}
