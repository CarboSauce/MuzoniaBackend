using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Muzonia.Core.Services;

namespace Muzonia.Core;

public static class ServiceExt
{
    public static void RegisterServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var types = assembly.GetTypes();

        foreach (
            var type in types.Where(t =>
                typeof(ISingleton).IsAssignableFrom(t)
                && t is { IsClass: true, IsAbstract: false }
            )
        )
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i != typeof(ISingleton));

            bool hasIterated = false;

            foreach (var serviceType in interfaces)
            {
                services.AddSingleton(serviceType, type);
                hasIterated = true;
            }

            if (!hasIterated)
            {
                services.AddSingleton(type);
            }
        }

        // Register types that implement ITransient
        foreach (
            var type in types.Where(t =>
                typeof(ITransient).IsAssignableFrom(t)
                && t is { IsClass: true, IsAbstract: false }
            )
        )
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i != typeof(ITransient));

            bool hasIterated = false;

            foreach (var serviceType in interfaces)
            {
                services.AddTransient(serviceType, type);
                hasIterated = true;
            }

            if (!hasIterated)
            {
                services.AddTransient(type);
            }
        }

        // Register types that implement IScoped
        foreach (
            var type in types.Where(t =>
                typeof(IScoped).IsAssignableFrom(t)
                && t is { IsClass: true, IsAbstract: false }
            )
        )
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i != typeof(IScoped));

            bool hasIterated = false;
            foreach (var serviceType in interfaces)
            {
                services.AddScoped(serviceType, type);
                hasIterated = true;
            }

            if (!hasIterated)
            {
                services.AddScoped(type);
            }
        }
    }
}
