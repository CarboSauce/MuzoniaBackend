using System.Diagnostics;
using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using Muzonia.Core.Common;

namespace Muzonia.Core.Services.Transcoding;

public struct TrackReturn
{
    public Uri MasterPlaylist;
    public TimeSpan Duration;
}

internal static partial class DurationRegex
{
    [GeneratedRegex(@"Duration: (\d+):(\d+):(\d+).(\d+),")]
    internal static partial Regex Regex { get; }
}

public interface ITrackTranscoderEngine
{
    Task<TrackReturn> Transcode(EntityId trackId);
}

internal class TrackTranscoderEngineFfmpeg(
    IOptions<ApiConfig> config,
    BlobServiceClient blobServiceClient
) : ITrackTranscoderEngine, IScoped
{
    private readonly ApiConfig config = config.Value;
    private const string ContainerName = "tracks";
    private const string ContainerNameOut = "music";

    public async Task<TrackReturn> Transcode(EntityId trackId)
    {
        var container = blobServiceClient.GetBlobContainerClient("tracks");
        await container.CreateIfNotExistsAsync();

        var sourceBlob = container.GetBlobClient($"tracks/{trackId}");

        if (!await sourceBlob.ExistsAsync())
        {
            throw new FileNotFoundException(
                $"Source blob not found: tracks/{trackId}"
            );
        }

        var tmpDir = Path.Combine(
            Path.GetTempPath(),
            "muzonia",
            Guid.NewGuid().ToString()
        );
        Directory.CreateDirectory(tmpDir);
        var inputFile = Path.Combine(tmpDir, "input_file");

        await using (var inputStream = File.Create(inputFile))
        {
            await sourceBlob.DownloadToAsync(inputStream);
        }

        // csharpier-ignore
        string ffmpegParams =
           $" -i \"{inputFile}\" -c:a aac -map a:0 -b:a:0 64k -f hls -hls_time 10 -hls_playlist_type vod"
         + " -map a:0 -b:a:1 128k -f hls -hls_time 10 -hls_playlist_type vod"
         + " -map a:0 -b:a:2 192k -f hls -hls_time 10 -hls_playlist_type vod"
         + " -var_stream_map \"a:0,name:64k a:1,name:128k a:2,name:192k\""
         + " -hls_segment_filename \"stream_%v_%03d.aac\""
         + " -master_pl_name \"master.m3u8\""
         + " \"stream_%v.m3u8\"";

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = config.FfmpegPath,
            Arguments = ffmpegParams,
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = tmpDir,
        };

        process.Start();

        TimeSpan? duration = null;
        while (await process.StandardError.ReadLineAsync() is { } line)
        {
            var match = DurationRegex.Regex.Match(line);
            if (match.Success)
            {
                duration = new TimeSpan(
                    0,
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value),
                    int.Parse(match.Groups[4].Value)
                );
            }
        }

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception(
                $"ffmpeg exited with status code {process.ExitCode}"
            );
        }

        if (duration is null)
        {
            throw new Exception("ffmpeg did not output duration");
        }

        var masterUri = await UploadAllFiles(tmpDir, trackId);

        try
        {
            Directory.Delete(tmpDir, recursive: true);
        }
        catch
        {
            // best-effort cleanup
        }

        return new TrackReturn
        {
            MasterPlaylist = masterUri,
            Duration = duration.Value,
        };
    }

    private async Task<Uri> UploadAllFiles(string tmpDir, EntityId trackId)
    {
        var container = blobServiceClient.GetBlobContainerClient(
            ContainerNameOut
        );
        await container.CreateIfNotExistsAsync();
        await container.SetAccessPolicyAsync(PublicAccessType.Blob);
        Uri? masterUri = null;
        var trackPrefix = $"{trackId}/";

        foreach (
            var file in Directory.EnumerateFiles(
                tmpDir,
                "*",
                SearchOption.TopDirectoryOnly
            )
        )
        {
            var relativePath = Path.GetRelativePath(tmpDir, file);

            if (relativePath == "input_file")
            {
                continue;
            }

            var blobName = $"{trackPrefix}{relativePath}";
            var blob = container.GetBlobClient(blobName);

            await using var stream = File.OpenRead(file);
            await blob.UploadAsync(stream, overwrite: true);

            if (relativePath == "master.m3u8")
            {
                masterUri = blob.Uri;
            }
        }

        return masterUri ?? throw new Exception("master.m3u8 upload failed");
    }
}
