using Muzonia.Api.GraphQL;
using Muzonia.Api.GraphQL.Mutation;
using Muzonia.Api.GraphQL.Query;
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
            .AddMaxExecutionDepthRule(6)
            .ModifyCostOptions(opt =>
            {
                opt.EnforceCostLimits = true;
                opt.ApplyCostDefaults = true;
            })
            .AddAuthorization()
            .AddApiTypes()
            .AddQueryType()
            .AddMutationType()
            .MapGraphqlTypes()
            .AddPagingArguments()
            .AddQueryContext()
            .AddSorting()
            .AddFiltering()
            .AddProjections();

        return services;
    }

    public static IApplicationBuilder UseGraphQl(this IApplicationBuilder app)
    {
        return app;
    }
}
