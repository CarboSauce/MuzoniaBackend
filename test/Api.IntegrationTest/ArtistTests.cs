using System.Net.Http.Json;
using Muzonia.Api.Features.Account;
using Muzonia.Core.Dto.Request;
using Shouldly;

namespace Muzonia.Api.IntegrationTest;

internal record ArtistResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string Description,
    Uri ImageUri,
    DateTime CreationDate
);

public class ArtistTests
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
    public async Task CreateArtist_Success()
    {
        var file = Utils.DummyFormFile();

        using var form = new MultipartFormDataContent();

        form.Add(
            new StreamContent(file.OpenReadStream())
            {
                Headers =
                {
                    ContentLength = file.Length,
                    ContentType = new("image/jpeg"),
                    ContentDisposition = new("form-data")
                    {
                        Name = "file",
                        FileName = file.FileName
                    }
                }
            },
            "file",
            file.FileName
        );

        form.Add(new StringContent("Test Artist"), "name");
        form.Add(new StringContent("Test Description"), "description");

        var response = await client.PostAsync("/artists", form);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ArtistResponse>();

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Test Artist");
        result.Description.ShouldBe("Test Description");
        result.CreationDate.ShouldNotBe(default);
    }

    [Test]
    [DependsOn(nameof(CreateArtist_Success))]
    public async Task GetMyArtist_Success()
    {
        var response = await client.GetAsync("/artists/me");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ArtistResponse>();

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Test Artist");
        result.Description.ShouldBe("Test Description");
        result.CreationDate.ShouldNotBe(default);
    }

    [Test]
    [DependsOn(nameof(CreateArtist_Success))]
    public async Task SearchArtist_Success()
    {
        var response = await client.GetAsync("/artists/search/Test");
        response.EnsureSuccessStatusCode();
        var result =
            await response.Content.ReadFromJsonAsync<ArtistResponse[]>();

        result.ShouldNotBeNull();
        result.ShouldContain(x => x.Name == "Test Artist");
        result.ShouldContain(x => x.Description == "Test Description");
    }
}
