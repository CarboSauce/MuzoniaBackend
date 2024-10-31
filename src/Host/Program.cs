using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var useAspirePostgres = builder
    .Configuration.GetSection("Aspire")
    .GetSection("UsePostgres")
    .Get<bool>();
var useAspireRedis = builder
    .Configuration.GetSection("Aspire")
    .GetSection("UseRedis")
    .Get<bool>();

if (useAspirePostgres)
{
    Console.WriteLine("Using Aspire Postgres");
}
else
{
    Console.WriteLine("Using Local Postgres");
}

if (useAspireRedis)
{
    Console.WriteLine("Using Aspire Redis");
}
else
{
    Console.WriteLine("Using Local Redis");
}

var coreapi = builder
    .AddProject<Projects.Api>("coreapi")
    .WithEnvironment("Aspire:UseAspire", "true")
    .WithEnvironment("Aspire:UsePostgres", useAspirePostgres.ToString())
    .WithEnvironment("Aspire:UseRedis", useAspireRedis.ToString());

if (useAspirePostgres)
{
    var postgres = builder
        .AddPostgres("postgres")
        .WithDataVolume(isReadOnly: false)
        .WithPgAdmin();

    var db = postgres.AddDatabase("apidb");
    _ = coreapi.WithReference(db);
}
else
{
    var con = builder.AddConnectionString("apidb");
    _ = coreapi.WithReference(con);
}

if (useAspireRedis)
{
    var cache = builder.AddRedis("redis");
    _ = coreapi.WithReference(cache);
}
else
{
    //var redisConString = builder.Configuration.GetConnectionString("redis");
    var con = builder.AddConnectionString("redis");
    _ = coreapi.WithReference(con);
}

builder.Build().Run();
