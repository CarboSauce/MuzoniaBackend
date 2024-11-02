namespace Muzonia.DbEf.Entities;

public class ArtistAlbum
{
    public Ulid ArtistId { get; set; }
    public Ulid AlbumId { get; set; }
    public required Artist Artist { get; set; }
    public required Album Album { get; set; }
}
