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

public class OnArtistImageUploaded(
    ILogger<OnArtistImageUploaded> logger,
    BlobServiceClient blobServiceClient,
    ApiDbContext dbContext
)
{
    public sealed record FinalizeRequest(Guid ArtistId);

    [Function(nameof(OnArtistImageUploaded))]
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

        if (data?.ArtistId == Guid.Empty)
        {
            return new BadRequestObjectResult("artistId required");
        }

        var container = blobServiceClient.GetBlobContainerClient("artists");
        var sourceBlob = container.GetBlobClient($"artists/{data.ArtistId}");

        if (!await sourceBlob.ExistsAsync())
        {
            return new NotFoundObjectResult("Blob not found");
        }

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

        var jpgBlobName = Path.ChangeExtension(data.ArtistId.ToString(), ".jpg");
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

        var blobUri = jpgBlob.Uri;
        await dbContext
            .Artists.Where(a => a.Id == data.ArtistId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.ImageUri, blobUri));

        logger.LogInformation("OnArtistImageUploaded fired for {ArtistId}", data.ArtistId);
        return new OkObjectResult(new { blob = jpgBlob });
    }
}