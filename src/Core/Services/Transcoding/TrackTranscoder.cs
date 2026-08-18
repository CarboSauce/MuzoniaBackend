using Muzonia.DbEf;

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
        var track = await dbContext.Tracks.FindAsync(trackId)
            ?? throw new FileNotFoundException($"Track not found: {trackId}");

        var result = await transcoderEngine.Transcode(trackId);

        track.DataUri = result.MasterPlaylist;
        track.Duration = (long)Math.Round(result.Duration.TotalSeconds);

        await dbContext.SaveChangesAsync();
    }
}