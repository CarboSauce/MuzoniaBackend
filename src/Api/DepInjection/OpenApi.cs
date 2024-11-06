using Muzonia.Api.Utils;

namespace Muzonia.Api.DepInjection;

internal static class OpenApi
{
    public static IServiceCollection AddOpenApi(
        this IServiceCollection services,
        IConfiguration config,
        IWebHostEnvironment env
    )
    {
        services.AddEndpointsApiExplorer();
        if (env.IsDevelopment())
        {
            services.AddHttpLogging(o => { });
            services.AddSwaggerGen(o =>
            {
                //o.MapType<Ulid>(() => new OpenApiSchema { Type = "string" });
                o.OperationFilter<HttpResultsOperationFilter>();
            });
            // services.AddOpenApiDocument(o =>
            // {
            //     o.Title = "Muzonia API";
            // });
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

        app.UseSwagger(o =>
        {
            o.RouteTemplate = "/openapi/{documentName}.json";
        });
        app.UseSwaggerUI(o =>
        {
            o.SwaggerEndpoint("/openapi/v1.json", "Muzonia API");
        });

        router
            .MapGet("/", () => Results.Redirect("/swagger"))
            .ExcludeFromDescription();
        router
            .MapGet("/apiui", () => Results.Redirect("/swagger"))
            .ExcludeFromDescription();

        return app;
    }
}
