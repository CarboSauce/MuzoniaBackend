namespace Muzonia.DbEf.Entities;

public class PlaylistSong
{
    public Ulid PlaylistId { get; set; }
    public Ulid SongId { get; set; }
    public required Playlist Playlist { get; set; }
    public required Song Song { get; set; }
}
