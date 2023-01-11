using microservice.toolkit.core.attribute;
using microservice.toolkit.core.extension;
using microservice.toolkit.messagemediator.collection;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Reflection;

namespace microservice.toolkit.messagemediator.extension;

public static class MessageMediatorExtensions
{

    /// <summary>
    /// Returns the implementations of Service&lt;,&gt; found in assembly in which the <i>type</i> is defined.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static MicroserviceCollection GetServices(this Type type)
    {
        return new[] { type }.GetServices();
    }

    public static MicroserviceCollection GetServices(this Type[] types)
    {
        return types.Select(t => Assembly.GetAssembly(t)).ToArray().GetServices();
    }

    /// <summary>
    /// Returns the implementations of Service&lt;,&gt; found in the assembly.
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static MicroserviceCollection GetServices(this Assembly assembly)
    {
        return new[] { assembly }.GetServices();
    }

    public static MicroserviceCollection GetServices(this Assembly[] assemblies)
    {
        return assemblies
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t.IsService())
            .ToMicroserviceCollection();
    }

    public static IServiceCollection AddServiceContext(this IServiceCollection services, Type[] types, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        var mapper = types.GetServices();

        services.AddServices(mapper, lifeTime);
        services.AddServiceProvider(mapper);

        return services;
    }

    public static IServiceCollection AddServiceContext(this IServiceCollection services, Type type, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        var mapper = type.GetServices();

        services.AddServices(mapper, lifeTime);
        services.AddServiceProvider(mapper);

        return services;
    }

    public static IServiceCollection AddServiceContext(this IServiceCollection services, Assembly[] assemblies, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        var mapper = assemblies.GetServices();

        services.AddServices(mapper, lifeTime);
        services.AddServiceProvider(mapper);

        return services;
    }

    public static IServiceCollection AddServiceContext(this IServiceCollection services, Assembly assembly, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        var mapper = assembly.GetServices();

        services.AddServices(mapper, lifeTime);
        services.AddServiceProvider(mapper);

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services, MicroserviceCollection mapper, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        foreach (var item in mapper.ToDictionary())
        {
            services.Add(new ServiceDescriptor(item.Value, item.Value, lifeTime));
        }

        return services;
    }

    public static IServiceCollection AddServiceProvider(this IServiceCollection services, MicroserviceCollection mapper)
    {
        services.AddSingleton(serviceProvider => new ServiceFactory(pattern =>
        {
            var serviceType = mapper.ByPatternOrDefault(pattern);

            return serviceProvider.GetService(serviceType) as IService;
        }));

        return services;
    }

    private static bool IsService(this Type type)
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
        var attrs = Attribute.GetCustomAttributes(type);

        if (attrs.Any(a => a is Microservice) == false)
        {
            return false;
        }

        // Checks if is a subclass of "Service<,>"
        var fullname = typeof(Service<,>).FullName;

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
