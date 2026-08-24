using CookieCrumble;

namespace Muzonia.Api.IntegrationTest;

[ClassDataSource<AspireHost>(Shared = SharedType.PerTestSession)]
[DependsOn(typeof(ArtistMutationTest))]
public class TrackMutationTests(AspireHost aspireHost)
{
    [Test]
    public async Task CreateTrack_Success(CancellationToken ct)
    {
        var client = aspireHost.CreateApiClient();

        var resp = await client.ExecuteAsync(
            """
            mutation {
                createTrack(input: { albumId: null, genre: "Pop", title: "Test track" }) {
                    track {
                        primaryArtist {
                            name description
                        }
                        genre title
                    }
                }
            }
            """
        );

        resp.EnsureSuccessStatusCode();

        var result = await resp.ReadAsResultAsync(ct);
        result.MatchSnapshot(extension: ".json");
    }
}
