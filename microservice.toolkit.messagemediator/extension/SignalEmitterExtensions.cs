using microservice.toolkit.core.attribute;
using microservice.toolkit.core.extension;
using microservice.toolkit.messagemediator.collection;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Reflection;

namespace microservice.toolkit.messagemediator.extension;

public static class SignalEmitterExtensions
{

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
        return types.Select(Assembly.GetAssembly).ToArray().GetHandlers();
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
            var serviceType = mapper.ByPatternOrDefault(pattern)
                .Select(type=> serviceProvider.GetService(type) as ISignalHandler)
                .ToArray();

            return serviceType;
        }));

        return services;
    }

    private static bool IsHandler(this Type type)
    {
        if (type == null)
        {
            return false;
        }

        if (type.IsClass == false || type.IsAbstract || type.IsGenericType || type.IsNested)
        {
            return false;
        }

        // Check for "MicroService" attribute
        // var attrs = Attribute.GetCustomAttributes(type);
        //
        // if (attrs.Any(a => a is Microservice) == false)
        // {
        //     return false;
        // }

        // Checks if is a subclass of "SignalHandler<>"
        var fullname = typeof(SignalHandler<>).FullName;

        if (fullname.IsNullOrEmpty())
        {
            return false;
        }

        var name = fullname.Substring(0, fullname.IndexOf('`'));
        var currentType = type;
        while (currentType != null)
        {
            if (currentType.BaseType?.FullName?.StartsWith(name) == true && currentType.BaseType.IsAbstract)
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }
}
