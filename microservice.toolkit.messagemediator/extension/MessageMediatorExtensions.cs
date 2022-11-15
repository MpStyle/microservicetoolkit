using microservice.toolkit.core.extension;
using microservice.toolkit.messagemediator.attribute;
using microservice.toolkit.messagemediator.collection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System;
using System.Linq;
using System.Reflection;

namespace microservice.toolkit.messagemediator.extension;

public static class MessageMediatorExtensions
{
    public static string ToPattern(this Type type)
    {
        var attrs = Attribute.GetCustomAttributes(type).FirstOrDefault(x => x is MicroService) as MicroService;

        if (attrs?.Pattern.IsNullOrEmpty() == false)
        {
            return attrs.Pattern;
        }

        return type.FullName.Replace(".", "/");
    }

    /// <summary>
    /// Returns the implementations of Service&lt;,&gt; found in assembly in which the <i>type</i> is defined.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static MicroserviceCollection GetServices(this Type type)
    {
        var assembly = Assembly.GetAssembly(type);

        return assembly.GetServices();
    }

    public static MicroserviceCollection GetServices(this Type[] types)
    {
        return types
            .Select(type => type.GetServices())
            .ToMicroserviceCollection();
    }

    /// <summary>
    /// Returns the implementations of Service&lt;,&gt; found in the assembly.
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static MicroserviceCollection GetServices(this Assembly assembly)
    {
        return assembly
            .GetExportedTypes()
            .Where(t => t.IsService())
            .ToMicroserviceCollection();
    }

    public static MicroserviceCollection GetServices(this Assembly[] assemblies)
    {
        return assemblies
            .Select(assembly => assembly.GetServices())
            .ToMicroserviceCollection();
    }

    public static ServiceCollection AddServiceContext(this ServiceCollection services, Type[] types, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        var mapper = types.GetServices();

        services.AddServices(mapper, lifeTime);
        services.AddServiceProvider(mapper);

        return services;
    }

    public static ServiceCollection AddServiceContext(this ServiceCollection services, Type type, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        var mapper = type.GetServices();

        services.AddServices(mapper, lifeTime);
        services.AddServiceProvider(mapper);

        return services;
    }

    public static ServiceCollection AddServiceContext(this ServiceCollection services, Assembly[] assemblies, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        var mapper = assemblies.GetServices();

        services.AddServices(mapper, lifeTime);
        services.AddServiceProvider(mapper);

        return services;
    }

    public static ServiceCollection AddServiceContext(this ServiceCollection services, Assembly assembly, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        var mapper = assembly.GetServices();

        services.AddServices(mapper, lifeTime);
        services.AddServiceProvider(mapper);

        return services;
    }

    public static ServiceCollection AddServices(this ServiceCollection services, MicroserviceCollection mapper, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        foreach (var item in mapper.ToDictionary())
        {
            services.Add(new ServiceDescriptor(item.Value, item.Value, lifeTime));
        }

        return services;
    }

    public static ServiceCollection AddServiceProvider(this ServiceCollection services, MicroserviceCollection mapper)
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

        if (attrs.Any(a => a is MicroService) == false)
        {
            return false;
        }

        // Checks if is a subclass of "Service<,>"
        var fullname = typeof(Service<,>).FullName;

        if (fullname.IsNullOrEmpty())
        {
            return false;
        }

        var name = fullname[..fullname.IndexOf('`')];
        var currentType = type;
        while (currentType != null)
        {
            if (currentType.BaseType?.FullName?.StartsWith(name) == true)
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }
}
