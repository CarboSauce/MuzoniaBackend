using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Muzonia.Api.Features.Account;
using Muzonia.Api.Features.User;
using Muzonia.DbEf;
using Shouldly;

namespace Muzonia.Api.IntegrationTest;

public class UserTests
{
    [ClassDataSource<WebAppFactory>(Shared = SharedType.PerTestSession)]
    public required WebAppFactory WebAppFactory { get; init; }

    private HttpClient client = null!;

    [Before(Test)]
    public async Task Setup()
    {
        client = WebAppFactory.CreateClient();
    }

    [Test]
    public async Task RegisterUser_Success()
    {
        var response = await client.PostAsJsonAsync(
            "/accounts/register",
            new RegisterUser.Request("test", "test@test.test", "test")
        );

        response.EnsureSuccessStatusCode();
        var result =
            await response.Content.ReadFromJsonAsync<RegisterUser.Response>();

        result.ShouldNotBeNull();
        result.Email.ShouldBe("test@test.test");
        result.Username.ShouldBe("test");
        result.CreationDate.ShouldNotBe(default);
    }

    [Test]
    [DependsOn(nameof(RegisterUser_Success))]
    public async Task LoginUser_Success()
    {
        var response = await client.PostAsJsonAsync(
            "/accounts/login",
            new LoginUser.Request("test", "test", true)
        );
        // Check if the Set-Cookie header is present
        response.Headers.Contains("Set-Cookie").ShouldBeTrue();

        // Optionally, you can also check the content of the Set-Cookie header
        var setCookieHeader = response
            .Headers.GetValues("Set-Cookie")
            .FirstOrDefault();
        setCookieHeader.ShouldNotBeNull();
        setCookieHeader.ShouldContain("MuzoniaAuth"); // Replace with the actual cookie name you expect
    }

    [Test]
    [DependsOn(nameof(GetMyAccount_Success))]
    public async Task LoginUser_Failure()
    {
        var response = await client.PostAsJsonAsync(
            "/accounts/login",
            new LoginUser.Request("test", "wrongpassword", true)
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    [DependsOn(nameof(RegisterUser_Success))]
    public async Task GetMyAccount_Success()
    {
        await client.PostAsJsonAsync(
            "/accounts/login",
            new LoginUser.Request("test", "test", true)
        );
        var response = await client.GetAsync("/users");
        response.EnsureSuccessStatusCode();
        var result =
            await response.Content.ReadFromJsonAsync<GetUserInfo.Response>();

        result.ShouldNotBeNull();
        result.Email.ShouldBe("test@test.test");
        result.Username.ShouldBe("test");
    }
}
