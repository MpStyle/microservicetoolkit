﻿using microservice.toolkit.core.extension;
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
        return new[] {type}.GetServices();
    }

    public static MicroserviceCollection GetServices(this Type[] types)
    {
        return types.Select(Assembly.GetAssembly).ToArray().GetServices();
    }

    /// <summary>
    /// Returns the implementations of Service&lt;,&gt; found in the assembly.
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static MicroserviceCollection GetServices(this Assembly assembly)
    {
        return new[] {assembly}.GetServices();
    }

    public static MicroserviceCollection GetServices(this Assembly[] assemblies)
    {
        return assemblies
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t.IsService())
            .ToMicroserviceCollection();
    }

    public static IServiceCollection AddServiceContext(
        this IServiceCollection services,
        Type[] types,
        ServiceLifetime servicesLifeTime = ServiceLifetime.Singleton,
        ServiceLifetime serviceProviderLifeTime = ServiceLifetime.Singleton
    )
    {
        var mapper = types.GetServices();

        services.AddServices(mapper, servicesLifeTime);
        services.AddServiceProvider(mapper, serviceProviderLifeTime);

        return services;
    }

    public static IServiceCollection AddServiceContext(this IServiceCollection services, Type type,
        ServiceLifetime servicesLifeTime = ServiceLifetime.Singleton,
        ServiceLifetime serviceProviderLifeTime = ServiceLifetime.Singleton)
    {
        var mapper = type.GetServices();

        services.AddServices(mapper, servicesLifeTime);
        services.AddServiceProvider(mapper, serviceProviderLifeTime);

        return services;
    }

    public static IServiceCollection AddServiceContext(this IServiceCollection services, Assembly[] assemblies,
        ServiceLifetime servicesLifeTime = ServiceLifetime.Singleton,
        ServiceLifetime serviceProviderLifeTime = ServiceLifetime.Singleton)
    {
        var mapper = assemblies.GetServices();

        services.AddServices(mapper, servicesLifeTime);
        services.AddServiceProvider(mapper, serviceProviderLifeTime);

        return services;
    }

    public static IServiceCollection AddServiceContext(this IServiceCollection services, Assembly assembly,
        ServiceLifetime servicesLifeTime = ServiceLifetime.Singleton,
        ServiceLifetime serviceProviderLifeTime = ServiceLifetime.Singleton)
    {
        var mapper = assembly.GetServices();

        services.AddServices(mapper, servicesLifeTime);
        services.AddServiceProvider(mapper, serviceProviderLifeTime);

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services, MicroserviceCollection mapper,
        ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        foreach (var type in mapper.ToDictionary().SelectMany(item => item.Value))
        {
            services.Add(new ServiceDescriptor(type, type, lifeTime));
        }

        return services;
    }

    public static IServiceCollection AddServiceProvider(this IServiceCollection services, MicroserviceCollection mapper,
        ServiceLifetime serviceProviderLifeTime = ServiceLifetime.Singleton)
    {
        services.Add(new ServiceDescriptor(typeof(IServiceProvider), serviceProvider => new ServiceFactory(pattern =>
        {
            var serviceTypes = mapper.ByPatternOrDefault(pattern);

            if (serviceTypes.IsNullOrEmpty())
            {
                return null;
            }

            return serviceProvider.GetService(serviceTypes.First()) as IService;
        }), serviceProviderLifeTime));

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
        // var attrs = Attribute.GetCustomAttributes(type);
        //
        // if (attrs.Any(a => a is Microservice) == false)
        // {
        //     return false;
        // }

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