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

public class OnPlaylistImageUploaded(
    ILogger<OnPlaylistImageUploaded> logger,
    BlobServiceClient blobServiceClient,
    ApiDbContext dbContext
)
{
    public sealed record FinalizeRequest(Guid PlaylistId);

    [Function(nameof(OnPlaylistImageUploaded))]
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

        if (data?.PlaylistId == Guid.Empty)
        {
            return new BadRequestObjectResult("playlistId required");
        }

        var container = blobServiceClient.GetBlobContainerClient("playlists");
        var sourceBlob = container.GetBlobClient($"playlists/{data.PlaylistId}");

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
                    Size = new Size(600, 600),
                    Mode = ResizeMode.Max,
                }
            );
        });

        var jpgBlobName = Path.ChangeExtension(data.PlaylistId.ToString(), ".jpg");
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
            .Playlists.Where(p => p.Id == data.PlaylistId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.ImageUri, blobUri));

        logger.LogInformation("OnPlaylistImageUploaded fired for {PlaylistId}", data.PlaylistId);
        return new OkObjectResult(new { blob = jpgBlob });
    }
}