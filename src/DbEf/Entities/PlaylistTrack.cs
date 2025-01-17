using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class PlaylistTrack : Entity
{
    public required EntityId PlaylistId { get; set; }
    public required EntityId TrackId { get; set; }
    public required int Index { get; set; }
    public Playlist Playlist { get; set; } = null!;
    public Track Track { get; set; } = null!;
}
