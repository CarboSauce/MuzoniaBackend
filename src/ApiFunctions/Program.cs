using Azure.Monitor.OpenTelemetry.Exporter;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muzonia.DbEf;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.AddServiceDefaults();

if (
    !string.IsNullOrEmpty(
        Environment.GetEnvironmentVariable(
            "APPLICATIONINSIGHTS_CONNECTION_STRING"
        )
    )
)
{
    builder
        .Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Services.AddDbContext<ApiDbContext>(o =>
{
    o.UseNpgsql(builder.Configuration.GetConnectionString("apidb"));
    o.UseExceptionProcessor();
});

builder.EnrichNpgsqlDbContext<ApiDbContext>();
builder.AddAzureBlobServiceClient("blobs");

builder.Build().Run();
