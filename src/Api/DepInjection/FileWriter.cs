using Muzonia.Core.Common;
using Muzonia.Core.Services;

namespace Muzonia.Api.DepInjection;

internal static class FileWriter
{
    public static IServiceCollection AddFileWriter(
        this IServiceCollection services,
        ApiConfig cfg,
        IWebHostEnvironment env
    )
    {
        if (cfg.UseStaticFiles)
        {
            if (env.IsDevelopment())
                services.AddDirectoryBrowser();
            services.Add<IFileWriter, StaticFileWriter>();
        }
        else
        {
            services.Add<IFileWriter, NoopFileWriter>();
        }
        return services;
    }
}
