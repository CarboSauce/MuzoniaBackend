using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Muzonia.Api.Services;
using Muzonia.Core.Common;
using Muzonia.DbEf;
using StackExchange.Redis;

namespace Muzonia.Api.DepInjection;

internal static class Database
{
    public static WebApplicationBuilder AddDatabase(
        this WebApplicationBuilder builder,
        IWebHostEnvironment env
    )
    {
        Action<DbContextOptionsBuilder> options = o =>
        {
            var conn = builder.Configuration.GetConnectionString("apidb");
            o.UseNpgsql(
                conn,
                o =>
                {
                    o.MigrationsAssembly("DbEf.Postgresql");
                    o.SetPostgresVersion(16, 0);
                }
            );
            o.UseExceptionProcessor();
        };

        builder.Services.AddPooledDbContextFactory<ApiDbContext>(options);
        builder.Services.AddScoped<ApiDbContext>(sp =>
        {
            var factory = sp.GetRequiredService<
                IDbContextFactory<ApiDbContext>
            >();
            return factory.CreateDbContext();
        });

        builder.EnrichNpgsqlDbContext<ApiDbContext>();

        return builder;
    }
}

internal static class Data
{
    public static WebApplicationBuilder AddAzure(
        this WebApplicationBuilder builder,
        IWebHostEnvironment env
    )
    {
        builder.AddAzureBlobServiceClient("blobs");

        builder.Services.AddHttpClient<FunctionsClient>(static client =>
        {
            client.BaseAddress = new("https+http://functions");
        });

        return builder;
    }
}

internal static class Redis
{
    public static WebApplicationBuilder AddRedis(
        this WebApplicationBuilder builder
    )
    {
        builder.AddRedisClient("redis");

        builder.Services.AddStackExchangeRedisCache(_ => { });
        builder
            .Services.AddOptions<RedisCacheOptions>()
            .Configure(
                (Action<RedisCacheOptions, IServiceProvider>)ConfigureOptions
            );

        return builder;
        static void ConfigureOptions(RedisCacheOptions o, IServiceProvider sp)
        {
            o.ConnectionMultiplexerFactory = () =>
                Task.FromResult(
                    sp.GetRequiredService<IConnectionMultiplexer>()
                );
        }
    }
}
