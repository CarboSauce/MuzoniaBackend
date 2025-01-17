using System.ComponentModel.DataAnnotations;

namespace Muzonia.DbEf.Entities;

public class TrackArtist
{
    public required EntityId TrackId { get; set; }
    public required EntityId ArtistId { get; set; }
    public Track Track { get; set; } = null!;
    public Artist Artist { get; set; } = null!;
}

// public class SongArtistConfig : IEntityTypeConfiguration<SongArtist>
// {
//     public void Configure(EntityTypeBuilder<SongArtist> builder)
//     {
//         builder.HasKey(e => new { e.SongId, e.ArtistId });
//         builder.HasOne(e => e.Song).Wi
//         builder
//             .HasOne(e => e.Artist)
//             .WithMany(e => e.Songs)
//             .HasForeignKey(e => e.ArtistId);
//     }
// }
