using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Muzonia.Core.Common;

namespace Muzonia.Api.DepInjection;

internal static class StaticFiles
{
    public static IApplicationBuilder AddStaticFiles(
        this IApplicationBuilder app,
        ApiConfig cfg,
        IWebHostEnvironment env
    )
    {
        if (cfg.UseStaticFiles is false)
            return app;

        var staticContentRoot = cfg.StaticContentRoot;

        if (staticContentRoot is not null)
            env.WebRootPath = staticContentRoot;

        var staticFileOptions = new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(env.WebRootPath, "muzonia")
            ),
            ContentTypeProvider = new FileExtensionContentTypeProvider(),
            RequestPath = "/static",
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.Append(
                    "Cache-Control",
                    "public, max-age=259200"
                );
            }
        };

        app.UseStaticFiles(staticFileOptions);

        if (env.IsDevelopment())
        {
            app.UseDirectoryBrowser(
                new DirectoryBrowserOptions
                {
                    FileProvider = staticFileOptions.FileProvider,
                    RequestPath = staticFileOptions.RequestPath
                }
            );
        }

        return app;
    }
}
