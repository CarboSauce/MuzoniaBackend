namespace Muzonia.DbEf.Entities;

public class ArtistAlbum
{
    public required EntityId ArtistId { get; set; }
    public required EntityId AlbumId { get; set; }
    public Artist Artist { get; set; } = null!;
    public Album Album { get; set; } = null!;
}
