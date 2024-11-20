using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class PlaylistTrack
{
    public required EntityId PlaylistId { get; set; }
    public required EntityId SongId { get; set; }
    public Playlist Playlist { get; set; } = null!;
    public Track Track { get; set; } = null!;
}
