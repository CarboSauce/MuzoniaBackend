using Hangfire;

namespace Muzonia.Core.Services.Transcoding;

public class TranscodeJob(ITrackTranscoder transcoder) : IScoped
{
    // [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public Task Run(EntityId trackId, EntityId fileId)
    {
        return transcoder.Transcode(trackId, fileId);
    }
}
