namespace Muzonia.DbEf.Entities;

public class PlaylistSong
{
    public required Guid PlaylistId { get; set; }
    public required Guid SongId { get; set; }
    public Playlist Playlist { get; set; } = null!;
    public Track Track { get; set; } = null!;
}
