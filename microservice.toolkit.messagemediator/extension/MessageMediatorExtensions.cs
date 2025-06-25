using microservice.toolkit.core;
using microservice.toolkit.core.entity;
using microservice.toolkit.core.extension;
using microservice.toolkit.messagemediator.collection;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.extension;

public static class MessageMediatorExtensions
{
    public static Task<ServiceResponse<TPayload>> Send<TRequest, TPayload>(this IMessageMediator messageMediator,
        Type serviceType, TRequest message)
    {
        return messageMediator.Send<TRequest, TPayload>(serviceType.ToPattern(), message);
    }

    public static Task<ServiceResponse<TPayload>> Send<TPayload>(this IMessageMediator messageMediator,
        Type serviceType, object message)
    {
        return messageMediator.Send<TPayload>(serviceType.ToPattern(), message);
    }

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TPayload"></typeparam>
    /// <param name="messageMediator"></param>
    /// <param name="pattern"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Task<ServiceResponse<TPayload>> Send<TRequest, TPayload>(this IMessageMediator messageMediator,
        string pattern, TRequest message)
    {
        return messageMediator.Send<TPayload>(pattern, message);
    }

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
        return types
            .Select(Assembly.GetAssembly)
            .Where(a => a != null)
            .ToArray()
            .GetServices();
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

    public static IServiceCollection AddServiceContext(
        this IServiceCollection services,
        Type[] types,
        ServiceLifetime servicesLifeTime = ServiceLifetime.Singleton,
        ServiceLifetime serviceProviderLifeTime = ServiceLifetime.Singleton
    )
    {
        var microserviceCollection = types.GetServices();

        return services
            .AddServices(microserviceCollection, servicesLifeTime)
            .AddServiceProvider(microserviceCollection, serviceProviderLifeTime);
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
        services.Add(new ServiceDescriptor(typeof(ServiceFactory), serviceProvider => new ServiceFactory(pattern =>
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
        if (type == null || !type.IsClass || type.IsAbstract)
        {
            return false;
        }

        var serviceGenericType = typeof(Service<,>);
        if (type.IsGenericType && type.GetGenericTypeDefinition() == serviceGenericType)
        {
            return true;
        }

        if (type.BaseType != null && type.BaseType.IsGenericType &&
            type.BaseType.GetGenericTypeDefinition() == serviceGenericType)
        {
            return true;
        }

        return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == serviceGenericType);
    }
}