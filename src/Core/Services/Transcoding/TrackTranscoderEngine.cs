using System.Diagnostics;
using System.Text.RegularExpressions;
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
    Task<TrackReturn> Transcode(
        string filename,
        EntityId trackId,
        Stream stream
    );
}

internal class TrackTranscoderEngineFfmpeg(
    IOptions<ApiConfig> config,
    IFileWriter fileWriter
) : ITrackTranscoderEngine, IScoped
{
    private readonly ApiConfig config = config.Value;

    public async Task<TrackReturn> Transcode(
        string filename,
        EntityId trackId,
        Stream stream
    )
    {
        string systemTmpDir = Path.GetTempPath();
        string tmpGuid = Guid.NewGuid().ToString();
        string tmpDir = Path.Combine(systemTmpDir, "muzonia", tmpGuid);

        Directory.CreateDirectory(tmpDir);

        // Write file from database to a file
        async Task<string> CreateInputFile()
        {
            using var file = File.Create(Path.Combine(tmpDir, "input_file"));
            await stream.CopyToAsync(file);
            await stream.FlushAsync();
            await file.FlushAsync();
            return file.Name;
        }
        var fileName = await CreateInputFile();
        // csharpier-ignore
        string ffmpegParams =
           $" -i \"{fileName}\" -c:a aac -map a:0 -b:a:0 64k -f hls -hls_time 10 -hls_playlist_type vod"
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
            WorkingDirectory = tmpDir
        };

        process.Start();

        // read line of stdout and check for regex match
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

        var statusCode = process.ExitCode;

        if (statusCode != 0)
        {
            throw new Exception($"ffmpeg exited with status code {statusCode}");
        }

        if (duration is null)
        {
            throw new Exception("ffmpeg did not output duration");
        }

        var uri = await UploadAllFiles(tmpDir, filename, trackId);

        return new TrackReturn
        {
            MasterPlaylist = uri,
            Duration = duration.Value,
        };
    }

    private async Task<Uri> UploadAllFiles(
        string tmpDir,
        string filename,
        EntityId trackId
    )
    {
        var files = Directory.EnumerateFiles(
            tmpDir,
            "*",
            SearchOption.TopDirectoryOnly
        );

        await fileWriter.CreateDirectory("tracks/", trackId.ToString());

        var trackUrlPrefix = $"tracks/{trackId.ToString()}/";

        Uri masterUri = null!;
        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(tmpDir, file);

            // Skip file called input_file and delete it later
            if (relativePath == "input_file")
            {
                continue;
            }
            await using var stream = File.OpenRead(file);

            var uri = await fileWriter.WriteAsync(
                stream,
                relativePath,
                trackUrlPrefix,
                ""
            );

            if (relativePath == "master.m3u8")
            {
                masterUri =
                    uri ?? throw new Exception("master.m3u8 upload failed");
            }
        }

        return masterUri;
    }
}
