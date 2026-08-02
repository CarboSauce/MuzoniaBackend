using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

#pragma warning disable ASPIREPERSISTENCE001
var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .WithPersistentLifetime()
    .AddDatabase("apidb");
#pragma warning restore ASPIREPERSISTENCE001

#pragma warning disable ASPIREPERSISTENCE001
var zitadel = builder
    .AddZitadel("zitadel")
    .WithDatabase(postgres)
    .WithPersistentLifetime();
zitadel.WaitFor(postgres);

#pragma warning disable ASPIREPERSISTENCE001
var cache = builder.AddRedis("redis").WithPersistentLifetime();

#pragma warning disable ASPIREPERSISTENCE001
var storage = builder
    .AddAzureStorage("storage")
    .RunAsEmulator(azurite =>
    {
        azurite.WithBlobPort(27000).WithQueuePort(27001).WithTablePort(27002);
    });
var coreapi = builder
    .AddProject<Projects.Api>("coreapi")
    .WithReference(cache)
    .WithReference(postgres)
    .WaitFor(cache)
    .WaitFor(zitadel)
    .WaitFor(postgres);

builder.Build().Run();
