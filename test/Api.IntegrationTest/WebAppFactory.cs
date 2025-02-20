using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Muzonia.Core.Services;
using Muzonia.Core.Services.Transcoding;
using Muzonia.DbEf;
using TUnit.Core.Interfaces;

namespace Muzonia.Api.IntegrationTest;

public class TestCleanupService(IServiceProvider provider) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("TestCleanupService.StopAsync");
        using var scope = provider.CreateScope();
        var dbContext =
            scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
    }
}

public class WebAppFactory : WebApplicationFactory<Program>, IAsyncInitializer
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(
            (_, config) =>
            {
                var curdir = Directory
                    .GetParent(AppContext.BaseDirectory)
                    ?.FullName;
                ArgumentNullException.ThrowIfNull(curdir);

                config.SetBasePath(curdir);
                config.AddJsonFile("testSettings.json");
            }
        );

        builder.ConfigureTestServices(services =>
        {
            services.Add<IDbFileService, NoopDbFileService>();
            services.Add<ITrackTranscoder, NoopTranscoder>();
            services.Add<ITranscodingService, NoopTranscodingService>();
            services.Add<IFileWriter, NoopFileWriter>();
            services.Add<IEmail, NoopEmail>();
        });

        builder.ConfigureServices(services =>
        {
            services.AddHostedService<TestCleanupService>();
        });

        base.ConfigureWebHost(builder);
    }

    public Task InitializeAsync()
    {
        _ = Server;

        return Task.CompletedTask;
    }
}
