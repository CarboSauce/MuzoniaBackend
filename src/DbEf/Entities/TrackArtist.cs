using System.ComponentModel.DataAnnotations;

namespace Muzonia.DbEf.Entities;

public class TrackArtist
{
    public EntityId SongId { get; set; }
    public EntityId ArtistId { get; set; }
    public required Track Track { get; set; }
    public required Artist Artist { get; set; }
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
