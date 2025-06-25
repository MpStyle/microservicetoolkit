using microservice.toolkit.core;
using microservice.toolkit.messagemediator.collection;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.extension;

public static class SignalEmitterExtensions
{
    public static Task Emit<TEvent>(this ISignalEmitter signalEmitter, Type signalEmitterType, TEvent message)
    {
        return signalEmitter.Emit(signalEmitterType.ToPattern(), message);
    }

    /// <summary>
    /// Returns the implementations of Service&lt;,&gt; found in assembly in which the <i>type</i> is defined.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static MicroserviceCollection GetHandlers(this Type type)
    {
        return new[] { type }.GetHandlers();
    }

    public static MicroserviceCollection GetHandlers(this Type[] types)
    {
        return types
            .Select(Assembly.GetAssembly)
            .Where(a => a != null)
            .Distinct()
            .ToArray()
            .GetHandlers();
    }

    /// <summary>
    /// Returns the implementations of Service&lt;,&gt; found in the assembly.
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static MicroserviceCollection GetHandlers(this Assembly assembly)
    {
        return new[] { assembly }.GetHandlers();
    }

    public static MicroserviceCollection GetHandlers(this Assembly[] assemblies)
    {
        return assemblies
            .Where(a => a != null)
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t.IsHandler())
            .ToMicroserviceCollection();
    }

    public static IServiceCollection AddHandlerContext(this IServiceCollection services, Type[] types, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        var mapper = types.GetServices();

        services.AddServices(mapper, lifeTime);
        services.AddServiceProvider(mapper);

        return services;
    }

    public static IServiceCollection AddHandlerContext(this IServiceCollection services, Type type, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        var mapper = type.GetServices();

        services.AddServices(mapper, lifeTime);
        services.AddServiceProvider(mapper);

        return services;
    }

    public static IServiceCollection AddHandlerContext(this IServiceCollection services, Assembly[] assemblies, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        var mapper = assemblies.GetServices();

        services.AddServices(mapper, lifeTime);
        services.AddServiceProvider(mapper);

        return services;
    }

    public static IServiceCollection AddHandlerContext(this IServiceCollection services, Assembly assembly, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        var mapper = assembly.GetServices();

        services.AddServices(mapper, lifeTime);
        services.AddServiceProvider(mapper);

        return services;
    }

    public static IServiceCollection AddHandlers(this IServiceCollection services, MicroserviceCollection mapper, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        foreach (var item in mapper.ToDictionary())
        {
            foreach (var type in item.Value)
            {
                services.Add(new ServiceDescriptor(type, type, lifeTime));
            }
        }

        return services;
    }

    public static IServiceCollection AddHandlerProvider(this IServiceCollection services, MicroserviceCollection mapper)
    {
        services.AddSingleton(serviceProvider => new SignalHandlerFactory(pattern =>
        {
            var serviceTypes = mapper.ByPatternOrDefault(pattern)
                .Select(type => serviceProvider.GetService(type) as ISignalHandler)
                .Where(handler => handler != null)
                .ToArray();

            return serviceTypes;
        }));

        return services;
    }

    private static bool IsHandler(this Type type)
    {
        if (type == null)
        {
            return false;
        }

        if (!type.IsClass || type.IsAbstract || type.IsGenericType || type.IsNested)
        {
            return false;
        }

        var handlerBaseType = typeof(SignalHandler<>);
        var currentType = type.BaseType;
        while (currentType != null)
        {
            if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == handlerBaseType)
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }
}
