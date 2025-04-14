using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Muzonia.Core.Common;
using Muzonia.DbEf;
using StackExchange.Redis;

namespace Muzonia.Api.DepInjection;

internal static class Database
{
    public static WebApplicationBuilder AddDatabase(
        this WebApplicationBuilder builder,
        AspireConfig aspireConfig,
        IWebHostEnvironment env
    )
    {
        if (aspireConfig.UsePostgres)
        {
            builder.AddNpgsqlDbContext<ApiDbContext>("apidb");
        }
        else
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
        }
        return builder;
    }
}

internal static class Redis
{
    public static WebApplicationBuilder AddRedis(
        this WebApplicationBuilder builder,
        AspireConfig aspireConfig
    )
    {
        if (aspireConfig.UseRedis)
        {
            builder.AddRedisClient("redis");
        }
        else
        {
            builder.Services.AddSingleton<IConnectionMultiplexer>(o =>
            {
                var redis = builder.Configuration.GetConnectionString("redis");
                ArgumentNullException.ThrowIfNull(redis);
                return ConnectionMultiplexer.Connect(redis);
            });
        }

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
