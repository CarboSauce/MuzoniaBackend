using Muzonia.Api.Middleware;
using Muzonia.Core.Common;

namespace Muzonia.Api.DepInjection;

internal static class GraphQl
{
    public static IServiceCollection AddGraphQLConfig(
        this IServiceCollection services,
        ApiConfig cfg,
        IWebHostEnvironment env
    )
    {
        services
            .AddGraphQLServer()
            .AddGlobalObjectIdentification()
            .AddMutationConventions()
            .AddMaxExecutionDepthRule(20)
            .ModifyCostOptions(opt =>
            {
                opt.EnforceCostLimits = true;
                opt.ApplyCostDefaults = true;
            })
            .ModifyRequestOptions(opt =>
                opt.IncludeExceptionDetails = env.IsDevelopment()
            )
            .UseField<ValidationMiddleware>()
            .AddAuthorization()
            .AddPagingArguments()
            .AddQueryContext()
            .AddSorting()
            .AddFiltering()
            .AddProjections()
            .AddApiTypes();

        return services;
    }

    public static IApplicationBuilder UseGraphQl(this IApplicationBuilder app)
    {
        return app;
    }
}
