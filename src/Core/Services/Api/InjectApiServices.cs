using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Muzonia.Core.Common;
using Muzonia.Core.Services.Transcoding;

namespace Muzonia.Core.Services.Api;

public static class InjectApiServices
{
    public static IServiceCollection AddServices(
        this IServiceCollection services,
        ApiConfig apiConfig,
        IConfiguration config
    )
    {
        services.Add<ArtistService>().Add<AlbumService>().Add<UserService>();

        if (apiConfig.UseNoopEmail)
        {
            services.Add<IEmail, NoopEmail>();
        }
        else
        {
            services.Add<IEmail, SendgridEmail>();
        }

        services
            .Add<TranscodeJob>()
            .Add<IDbFileService, DbFileService>()
            .Add<ITranscodingService, HangfireTranscoder>()
            .Add<ITrackTranscoder, TrackTranscoder>()
            .Add<ITrackTranscoderEngine, TrackTranscoderEngineFfmpeg>();

        return services;
    }
}
