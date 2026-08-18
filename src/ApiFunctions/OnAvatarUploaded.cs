using System.Net;
using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muzonia.DbEf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ApiFunctions;

public class OnAvatarUploaded(
    ILogger<OnAvatarUploaded> logger,
    BlobServiceClient blobServiceClient,
    ApiDbContext dbContext
)
{
    public sealed record FinalizeRequest(Guid UserId);

    [Function(nameof(OnAvatarUploaded))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req
    )
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        FinalizeRequest? data;
        try
        {
            data = JsonSerializer.Deserialize<FinalizeRequest>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
        }
        catch (JsonException ex)
        {
            return new BadRequestObjectResult($"Invalid JSON: {ex.Message}");
        }

        if (string.IsNullOrEmpty(data?.UserId.ToString()))
        {
            return new BadRequestObjectResult("userId required");
        }

        logger.LogInformation($"GetBlobContainerClient {data.UserId}");
        var container = blobServiceClient.GetBlobContainerClient("users");

        logger.LogInformation($"GetBlobClient {data.UserId}");
        var sourceBlob = container.GetBlobClient(
            $"users/{data.UserId.ToString()}"
        );

        if (!await sourceBlob.ExistsAsync())
        {
            return new NotFoundObjectResult("Blob not found");
        }

        logger.LogInformation($"Downloading {data.UserId}");
        var download = await sourceBlob.DownloadStreamingAsync();

        await using var input = download.Value.Content;

        using var image = await Image.LoadAsync(input);

        image.Mutate(x =>
        {
            x.Resize(
                new ResizeOptions
                {
                    Size = new Size(300, 300),
                    Mode = ResizeMode.Max,
                }
            );
        });

        // New blob name
        var jpgBlobName = Path.ChangeExtension(data.UserId.ToString(), ".jpg");

        var jpgBlob = container.GetBlobClient(jpgBlobName);

        await using var output = new MemoryStream();

        await image.SaveAsJpegAsync(output, new JpegEncoder { Quality = 85 });

        output.Position = 0;

        await jpgBlob.UploadAsync(
            output,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "image/jpeg",
                },
            }
        );

        logger.LogInformation($"Uploaded with url {jpgBlob.Uri}");
        var blobUri = jpgBlob.Uri.ToString();

        var user = await dbContext
            .Users.Where(u => u.Id == data.UserId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.ImageUri, blobUri));

        return new OkObjectResult(new { blob = jpgBlob });
    }
}
