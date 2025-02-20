using System.Net.Http.Json;
using Muzonia.Api.Features.Account;
using Muzonia.Api.Features.Playlists;
using Shouldly;

namespace Muzonia.Api.IntegrationTest;

public class PlaylistTests
{
    [ClassDataSource<WebAppFactory>(Shared = SharedType.PerTestSession)]
    public required WebAppFactory WebAppFactory { get; init; }
    private HttpClient client = null!;

    [Before(Test)]
    public async Task Setup()
    {
        client = WebAppFactory.CreateClient();
        await client.PostAsJsonAsync(
            "/accounts/login",
            new LoginUser.Request("admin", "admin", true)
        );
    }

    [Test]
    public async Task CreatePlaylist_Success()
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("Test Playlist"), "name");
        form.Add(new StringContent("Test Description"), "description");
        form.Add(new StringContent("true"), "isPublic");

        var response = await client.PostAsync("/playlists", form);

        response.EnsureSuccessStatusCode();
        var result =
            await response.Content.ReadFromJsonAsync<CreatePlaylist.Response>();

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Test Playlist");
        result.Description.ShouldBe("Test Description");
    }

    [Test]
    [DependsOn(nameof(CreatePlaylist_Success))]
    public async Task GetPlaylist_Success()
    {
        var response = await client.GetAsync("/playlists");

        response.EnsureSuccessStatusCode();
        var result =
            await response.Content.ReadFromJsonAsync<GetMinePlaylists.Response[]>();

        result.ShouldNotBeNull();
        result.ShouldContain(r => r.Title == "Test Playlist");
        result.ShouldContain(r => r.Description == "Test Description");
    }

    [Test]
    [DependsOn(nameof(CreatePlaylist_Success))]
    public async Task SearchPlaylist_Success()
    {
        var response = await client.GetAsync("/playlists/search/Test");

        response.EnsureSuccessStatusCode();
        var result =
            await response.Content.ReadFromJsonAsync<SearchPlaylist.Response[]>();

        result.ShouldNotBeNull();
        result.ShouldContain(r => r.Title == "Test Playlist");
        result.ShouldContain(r => r.Description == "Test Description");
    }
}
