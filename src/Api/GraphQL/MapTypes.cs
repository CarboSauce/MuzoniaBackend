using HotChocolate.Execution.Configuration;

namespace Muzonia.Api.GraphQL;

public interface ITypeResolver;

public static class MapTypes
{
    public static IRequestExecutorBuilder MapGraphqlTypes(
        this IRequestExecutorBuilder services
    )
    {
        // find all types implementing ITypeResolver with reflection

        var types = AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(t =>
                t is { IsClass: true, IsAbstract: false }
                && typeof(ITypeResolver).IsAssignableFrom(t)
            )
            .ToList();

        foreach (var type in types)
        {
            services.AddTypeExtension(type);
        }

        return services;
    }
}
