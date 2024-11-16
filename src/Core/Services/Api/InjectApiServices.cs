using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Muzonia.Core.Common;

namespace Muzonia.Core.Services.Api;

public static class InjectApiServices
{
    public static IServiceCollection AddServices(
        this IServiceCollection services,
        ApiConfig apiConfig,
        IConfiguration config
    )
    {
        services
            .Add<ArtistService>()
            .Add<AlbumService>()
            .Add<UserService>()
            .Add<TrackService>()
            .Add<PlaylistService>()
            .Add<HistoryService>()
            .Add<QueueService>()
            .Add<FileService>();

        if (apiConfig.UseNoopEmail)
        {
            services.Add<IEmail, NoopEmail>();
        }
        else
        {
            services.Add<IEmail, SendgridEmail>();
        }

        return services;
    }
}
