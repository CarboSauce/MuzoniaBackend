using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

#pragma warning disable ASPIREPERSISTENCE001
var postgres = builder
    .AddPostgres("postgres")
    .WithPgWeb()
    .WithDataVolume()
    .WithPersistentLifetime()
    .AddDatabase("apidb");
#pragma warning restore ASPIREPERSISTENCE001

#pragma warning disable ASPIREPERSISTENCE001

#pragma warning disable ASPIREPERSISTENCE001
var cache = builder.AddRedis("redis").WithPersistentLifetime();

#pragma warning disable ASPIREPERSISTENCE001
var storage = builder
    .AddAzureStorage("storage")
    .RunAsEmulator(azurite =>
    {
        azurite.WithBlobPort(27000).WithQueuePort(27001).WithTablePort(27002);
    });

var authProjectDirectory = "../WebApp";

var migrateAuthExec = builder
    .AddExecutable(
        "migrate-auth",
        "pnpm",
        authProjectDirectory,
        ["pnpm", "exec", "drizzle-kit", "migrate"]
    )
    .WithWorkingDirectory(authProjectDirectory)
    .WithReference(postgres)
    .WaitFor(postgres)
    .WithEnvironment(context =>
    {
        context.EnvironmentVariables["POSTGRES_URI"] = postgres
            .Resource
            .UriExpression;
    });

var webAppPort = 3003;
var webAppSecret = builder.AddParameter("better-auth-secret", secret: true);

var authApi = builder
    .AddJavaScriptApp("auth", authProjectDirectory)
    .WithHttpEndpoint(port: webAppPort, env: "APP_PORT")
    .WithPnpm(install: false)
    .WithEnvironment("BETTER_AUTH_SECRET", webAppSecret)
    .WithEnvironment(context =>
    {
        context.EnvironmentVariables["POSTGRES_URI"] = postgres
            .Resource
            .UriExpression;
        context.EnvironmentVariables["BETTER_AUTH_URL"] =
            $"http://localhost:{webAppPort}";
    })
    .WaitForCompletion(migrateAuthExec);

var coreapi = builder
    .AddProject<Projects.Api>("coreapi")
    .WithReference(cache)
    .WithReference(postgres)
    .WaitFor(cache)
    .WaitFor(postgres)
    .WaitForCompletion(migrateAuthExec);

#pragma warning disable ASPIREDOCKERFILEBUILDER001
var caddy = builder
    .AddContainer("caddy", "caddy", "2.11")
    .WithBindMount("Caddyfile", "/etc/caddy/Caddyfile", isReadOnly: true)
    .WithReference(authApi)
    .WithReference(coreapi)
    .WithEnvironment(
        "AUTH_HOST",
        authApi.GetEndpoint("http").Property(EndpointProperty.Host)
    )
    .WithEnvironment(
        "AUTH_PORT",
        authApi.GetEndpoint("http").Property(EndpointProperty.Port)
    )
    .WithEnvironment(
        "API_HOST",
        coreapi.GetEndpoint("http").Property(EndpointProperty.Host)
    )
    .WithEnvironment(
        "API_PORT",
        coreapi.GetEndpoint("http").Property(EndpointProperty.Port)
    )
    .WithHttpEndpoint(port: 8080, targetPort: 80);

builder.Build().Run();
