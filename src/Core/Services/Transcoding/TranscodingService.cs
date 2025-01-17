using Hangfire;
using Microsoft.AspNetCore.Http;
using TranscodeId = string;

namespace Muzonia.Core.Services.Transcoding;

public enum TranscodingState
{
    Unknown,
    Processing,
    Completed,
    Failed
}

public interface ITranscodingService
{
    Task<TranscodeId> Enqueue(EntityId trackId, EntityId fileId);
    Task<TranscodingState> Poll(TranscodeId id);
}

public class HangfireTranscoder(IBackgroundJobClient client)
    : ITranscodingService,
        ISingleton
{
    public Task<TranscodeId> Enqueue(EntityId trackId, EntityId fileId)
    {
        var id = client.Enqueue<TranscodeJob>(transcoder =>
            transcoder.Run(trackId, fileId)
        );

        return Task.FromResult(id);
    }

    public Task<TranscodingState> Poll(TranscodeId id)
    {
        var state = JobStorage.Current.GetConnection().GetJobData(id);

        if (state is null)
        {
            return Task.FromResult(TranscodingState.Unknown);
        }

        var stateResult = state.State switch
        {
            "Processing" => TranscodingState.Processing,
            "Succeeded" => TranscodingState.Completed,
            "Failed" => TranscodingState.Failed,
            _ => TranscodingState.Unknown,
        };

        return Task.FromResult(stateResult);
    }
}
