using CookieCrumble;
using HotChocolate.Transport.Http;

namespace Muzonia.Api.IntegrationTest;

[ClassDataSource<AspireHost>(Shared = SharedType.PerTestSession)]
[DependsOn(typeof(AuthTest))]
public class ArtistMutationTest(AspireHost aspireHost)
{
    [Test]
    public async Task CreateArtist_Success(CancellationToken ct)
    {
        var client = aspireHost.CreateApiClient();

        var resp = await client.ExecuteAsync(
            """
            mutation {
                createArtist(input: { description: "TEST ARTIST", name: "TEST ARTIST"}){
                    artist {
                        name
                    }
                }
            }
            """,
            ct
        );

        resp.EnsureSuccessStatusCode();

        var result = await resp.ReadAsResultAsync(ct);
        result.MatchSnapshot(extension: ".json");
    }
}
