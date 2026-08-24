using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Projects;
using Shouldly;

namespace Muzonia.Api.IntegrationTest;

[ClassDataSource<AspireHost>(Shared = SharedType.PerTestSession)]
public class AuthTest(AspireHost aspireHost)
{
    [Test]
    public async Task RegisterUser_Success(CancellationToken ct)
    {
        var request = new
        {
            name = "TestUser",
            email = "TestUser@test.test",
            password = "@TestPassword123",
            callbackURL = "",
            rememberMe = true,
        };

        var action = async () =>
        {
            using var client = aspireHost.CreateAuthClient();
            var response = await client.PostAsJsonAsync(
                "api/auth/sign-up/email",
                request,
                ct
            );
            response.EnsureSuccessStatusCode();
        };
        await action.ShouldNotThrowAsync();
    }

    [Test]
    [DependsOn(nameof(RegisterUser_Success))]
    public async Task Login_Success(CancellationToken ct)
    {
        var action = async () =>
        {
            var request = new
            {
                email = "TestUser@test.test",
                password = "@TestPassword123",
                callbackUrl = "",
                rememberMe = true,
            };
            using var client = aspireHost.CreateAuthClient();
            var response = await client.PostAsJsonAsync(
                "api/auth/sign-in/email",
                request,
                ct
            );

            response.EnsureSuccessStatusCode();

            var resp = await client.GetFromJsonAsync<
                Dictionary<string, object>
            >("api/auth/token", ct);

            var token = resp?["token"] ?? throw new Exception("token is null");
            var tokenEle = (System.Text.Json.JsonElement)token;
            var loginToken = tokenEle.GetString();
            loginToken.ShouldNotBeNull();

            aspireHost.Token =
                loginToken ?? throw new InvalidOperationException();
        };

        await action.ShouldNotThrowAsync();
    }
}
