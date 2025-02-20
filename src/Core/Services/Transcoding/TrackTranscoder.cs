using Microsoft.EntityFrameworkCore;
using Muzonia.DbEf;
using Muzonia.DbEf.Entities;

namespace Muzonia.Core.Services.Transcoding;

public interface ITrackTranscoder
{
    Task Transcode(EntityId trackId, EntityId fileId);
}

public class NoopTranscoder : ITrackTranscoder, IScoped
{
    public Task Transcode(EntityId trackId, EntityId fileId) =>
        Task.CompletedTask;
}

public class TrackTranscoder(
    ITrackTranscoderEngine transcoderEngine,
    ApiDbContext dbContext
) : ITrackTranscoder, IScoped
{
    public async Task Transcode(EntityId trackId, EntityId fileId)
    {
        var file = await dbContext.Files.FindAsync(fileId);

        if (file is null)
        {
            throw new FileNotFoundException();
        }

        TrackReturn result;
        using (var stream = new MemoryStream(file.Data, 0, file.Length))
        {
            result = await transcoderEngine.Transcode(
                file.Name,
                trackId,
                stream
            );
        }

        var track = await dbContext.Tracks.FindAsync(trackId);
        if (track is null)
        {
            throw new FileNotFoundException();
        }

        track.DataUri = result.MasterPlaylist;
        track.Duration = (long)Math.Round(result.Duration.TotalSeconds);

        // dbContext.Files.Remove(file);
        await dbContext.SaveChangesAsync();
    }
}
