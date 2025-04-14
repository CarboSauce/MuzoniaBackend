namespace Muzonia.Api.GraphQL.Query;

public class TrackDto
{
    public string Title { get; set; }
    public string Genre { get; set; }
    public Uri? DataUri { get; set; }
    public long Duration { get; set; }
}
