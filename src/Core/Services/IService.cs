using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Muzonia.Core.Services.Api;

namespace Muzonia.Core.Services;

public interface IServiceInjection
{
    public static abstract void AddServices(
        IServiceCollection services,
        IConfiguration config
    );
}

public static class ServiceInjectionExt
{
    public static void RegisterFromAssembly(
        this IServiceCollection services,
        IConfiguration config,
        Assembly assembly
    )
    {
        var types = assembly.GetTypes();
        var serviceInjectionTypes = types
            .Where(t => t.GetInterfaces().Contains(typeof(IServiceInjection)))
            .ToList();
        foreach (var serviceInjectionType in serviceInjectionTypes)
        {
            serviceInjectionType
                .GetMethod("AddServices")
                ?.Invoke(null, [services, config]);
        }
    }
}

public static class ServiceProviderExtTransient
{
    public static IServiceCollection Add<TService, TImplementation>(
        this IServiceCollection services
    )
        where TService : class
        where TImplementation : class, TService, ITransient
    {
        return services.AddTransient<TService, TImplementation>();
    }

    public static IServiceCollection Add<TImplementation>(
        this IServiceCollection services
    )
        where TImplementation : class, ITransient
    {
        return services.AddTransient<TImplementation>();
    }
}

public static class ServiceProviderExtScoped
{
    public static IServiceCollection Add<TService, TImplementation>(
        this IServiceCollection services
    )
        where TService : class
        where TImplementation : class, TService, IScoped
    {
        return services.AddScoped<TService, TImplementation>();
    }

    public static IServiceCollection Add<TImplementation>(
        this IServiceCollection services
    )
        where TImplementation : class, IScoped
    {
        return services.AddScoped<TImplementation>();
    }
}

public static class ServiceProviderExtSingleton
{
    public static IServiceCollection Add<TService, TImplementation>(
        this IServiceCollection services
    )
        where TService : class
        where TImplementation : class, TService, ISingleton
    {
        return services.AddSingleton<TService, TImplementation>();
    }

    public static IServiceCollection Add<TImplementation>(
        this IServiceCollection services
    )
        where TImplementation : class, ISingleton
    {
        return services.AddSingleton<TImplementation>();
    }
}

public interface ITransient;

public interface IScoped;

public interface ISingleton;
