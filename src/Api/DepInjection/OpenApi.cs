using Scalar.AspNetCore;

namespace Muzonia.Api.DepInjection;

internal static class OpenApi
{
    public static IServiceCollection AddOpenApiServices(
        this IServiceCollection services,
        IConfiguration config,
        IWebHostEnvironment env
    )
    {
        services.AddEndpointsApiExplorer();
        if (env.IsDevelopment())
        {
            services.AddHttpLogging(o => { });
            services.AddOpenApi(o =>
            {
                // o.AddDocumentTransformer(
                //     (doc, _, _) =>
                //     {
                //         doc.Info.Title = "Muzonia Api";
                //         doc.Info.Version = "v1";
                //         doc.Info.Description = "Muzonia Api";
                //         return Task.CompletedTask;
                //     }
                // );
            });
        }
        return services;
    }

    public static IApplicationBuilder UseOpenApi(
        this IApplicationBuilder app,
        IEndpointRouteBuilder router,
        IConfiguration config,
        IWebHostEnvironment env
    )
    {
        if (!env.IsDevelopment())
        {
            return app;
        }
        app.UseDeveloperExceptionPage();
        // app.UseOpenApi(options =>
        // {
        //     options.Path = "/openapi/{documentName}.json";
        // });

        router.MapOpenApi();
        router.MapScalarApiReference(o =>
        {
            var serverUrl =
                $"{config["ApiProtocol"]}://{config["ApiDomain"]}:{config["ApiPort"]}";
            o.AddServer(serverUrl);
            o.Title = "Muzonia Api";
        });
        var reroute = () => Results.Redirect("/scalar/v1");
        router.MapGet("/", reroute).ExcludeFromDescription();
        router.MapGet("/apiui", reroute).ExcludeFromDescription();

        app.UseSwaggerUI(o =>
        {
            o.SwaggerEndpoint("/openapi/v1.json", "API");
        });

        return app;
    }
}
