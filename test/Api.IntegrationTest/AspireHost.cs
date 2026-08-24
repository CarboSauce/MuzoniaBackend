using System.Net;
using System.Net.Http.Headers;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.JavaScript;
using Aspire.Hosting.Testing;
using CookieCrumble;
using CookieCrumble.HotChocolate;
using HotChocolate.Transport.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TUnit.Core.Interfaces;

namespace Muzonia.Api.IntegrationTest;

public class AspireHost : IAsyncInitializer
{
    public DistributedApplication AppHost { get; private set; } = null!;
    public string Token { get; set; }

    public async Task InitializeAsync()
    {
        var ct = CancellationToken.None;

        var appHost =
            await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>(
                cancellationToken: ct,
                args: ["Testing=true"],
                configureBuilder: (opts, hostSettings) =>
                {
                    opts.DisableDashboard = false;
                }
            );

        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);

            logging.AddFilter(
                appHost.Environment.ApplicationName,
                LogLevel.Debug
            );

            logging.AddFilter("Aspire.", LogLevel.Debug);
        });

        appHost.Services.ConfigureHttpClientDefaults(client =>
        {
            client.AddStandardResilienceHandler();
        });

        var webApp = appHost.CreateResourceBuilder<JavaScriptAppResource>(
            "webapp"
        );

        webApp.WithEnvironment("VITE_IS_TESTING", "true");

        AppHost = await appHost.BuildAsync(ct);
        await AppHost.StartAsync(ct);

        new CookieCrumbleModule().Initialize();
        new CookieCrumbleHotChocolate().Initialize();
    }

    // private async Task InitDb()
    // {
    //     var connectionString = await AppHost.GetConnectionStringAsync(
    //         "postgres"
    //     );
    //     var csbuilder = new NpgsqlConnectionStringBuilder(connectionString);
    //     if (string.IsNullOrEmpty(csbuilder.Database))
    //     {
    //         csbuilder.Database = "postgres";
    //     }
    //
    //     var con = new NpgsqlConnection(csbuilder.ConnectionString);
    //
    //     respawner = await Respawner.CreateAsync(
    //         con,
    //         new RespawnerOptions { DbAdapter = DbAdapter.Postgres }
    //     );
    // }

    public GraphQLHttpClient CreateApiClient()
    {
        var client = AppHost.CreateHttpClient("caddy", "http");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Token);
        client.BaseAddress = new Uri(
            AppHost.GetEndpoint("caddy", "http"),
            "api/graphql"
        );
        return GraphQLHttpClient.Create(client);
    }

    public HttpClient CreateAuthClient()
    {
        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new CookieContainer(),
        };
        var client = new HttpClient(handler)
        {
            BaseAddress = AppHost.GetEndpoint("caddy", "http"),
        };
        return client;
    }
}
