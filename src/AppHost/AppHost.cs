using Azure.Provisioning;
using Azure.Provisioning.Storage;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var testing = builder.Configuration.GetValue("Testing", false);

var postgresServer = builder.AddPostgres("postgres").WithPgWeb();
if (!testing)
{
    postgresServer
        .WithDataVolume()
        .WithLifetime(
            testing ? ContainerLifetime.Session : ContainerLifetime.Persistent
        );
}
var postgres = postgresServer.AddDatabase("apidb");

var cache = builder
    .AddRedis("redis")
    .WithLifetime(
        testing ? ContainerLifetime.Session : ContainerLifetime.Persistent
    );

var storage = builder
    .AddAzureStorage("storage")
    .RunAsEmulator(azurite =>
    {
        azurite.WithBlobPort(10000).WithQueuePort(10001).WithTablePort(10002);
        if (!testing)
            azurite.WithDataVolume();
    });
var blobs = storage.AddBlobs("blobs");
var queue = storage.AddQueue("queue");

var authProjectDirectory = "../WebApp";
var clientId = builder.AddParameter("client-id");
var clientSecret = builder.AddParameter("client-secret");

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

var mailpit = builder.AddMailPit("mailpit");

if (!testing)
    mailpit
        .WithDataVolume("mailpit-data")
        .WithLifetime(
            testing ? ContainerLifetime.Session : ContainerLifetime.Persistent
        );

var webAppPort = 3003;
var webAppSecret = builder.AddParameter("better-auth-secret", secret: true);
var authAudience = builder.AddParameter("auth-audience", value: "muzonia-api");
var authApi = builder
    .AddJavaScriptApp("webapp", authProjectDirectory)
    .WithHttpEndpoint(port: webAppPort, env: "APP_PORT")
    .WithEnvironment("BETTER_AUTH_SECRET", webAppSecret)
    .WithEnvironment("AUTH_AUDIENCE", authAudience)
    .WithReference(mailpit)
    .WaitForCompletion(migrateAuthExec)
    .WithPnpm(install: false);

var coreapi = builder
    .AddProject<Projects.Api>("coreapi")
    .WithReference(blobs)
    .WithReference(cache)
    .WithReference(postgres)
    .WithEnvironment("Auth:Audience", authAudience)
    .WithEnvironment("Auth:ClientId", clientId)
    .WithEnvironment("Auth:ClientSecret", clientSecret)
    .WaitFor(cache)
    .WaitFor(postgres)
    .WaitFor(authApi)
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

authApi.WithEnvironment(context =>
{
    var port = caddy.GetEndpoint("http").Port;
    var host = caddy.GetEndpoint("http").Host;
    var scheme = caddy.GetEndpoint("http").Scheme;
    var url = $"{scheme}://{host}:{port}";

    context.EnvironmentVariables["POSTGRES_URI"] = postgres
        .Resource
        .UriExpression;
    context.EnvironmentVariables["BETTER_AUTH_URL"] = url;
    context.EnvironmentVariables["VITE_URL"] = url;
});

coreapi.WithEnvironment(
    "Auth:Authority",
    $"{caddy.GetEndpoint("http").Property(EndpointProperty.Scheme)}://{caddy.GetEndpoint("http").Property(EndpointProperty.Host)}:{caddy.GetEndpoint("http").Property(EndpointProperty.Port)}"
);

var functions = builder
    .AddAzureFunctionsProject(
        "functions",
        "../ApiFunctions/ApiFunctions.csproj"
    )
    .WithExternalHttpEndpoints()
    .WithReference(postgres)
    .WaitFor(storage)
    .WithHostStorage(storage)
    .WithReference(queue)
    .WithReference(blobs);

coreapi.WithReference(functions).WaitFor(functions);

builder.Build().Run();
