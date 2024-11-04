namespace Muzonia.DbEf.Entities;

public class ArtistAlbum
{
    public required Guid ArtistId { get; set; }
    public required Guid AlbumId { get; set; }
    public Artist Artist { get; set; } = null!;
    public Album Album { get; set; } = null!;
}
