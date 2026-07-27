using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("apidb");

var zitadel = builder.AddZitadel("zitadel").WithDatabase(postgres);
zitadel.WaitFor(postgres);

var cache = builder.AddRedis("redis");

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
    .WaitFor(storage)
    .WaitFor(zitadel)
    .WaitFor(postgres);

builder.Build().Run();
