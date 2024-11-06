using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Muzonia.Core.Services.Api;

public static class InjectApiServices
{
    public static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration config
    ) =>
        services
            .Add<ArtistService>()
            .Add<AlbumService>()
            .Add<UserService>()
            .Add<TrackService>()
            .Add<PlaylistService>()
            .Add<HistoryService>()
            .Add<QueueService>()
            .Add<FileService>();
}
