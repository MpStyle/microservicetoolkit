using microservice.toolkit.core.extension;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Reflection;

namespace microservice.toolkit.messagemediator.extension
{
    public static class AssemblyScannerExtension
    {
        /// <summary>
        /// Registers (as singleton instance, as default) every implementation of Service&lt;,&gt; found in assembly in which the specified type is defined.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="type"></param>
        /// <param name="lifeTime"></param>
        /// <returns></returns>
        public static IServiceCollection AddAssemblyServices(this IServiceCollection services, Type type, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
        {
            return services.AddServices(Assembly.GetAssembly(type),lifeTime);
        }

        /// <summary>
        /// Registers (as singleton instance, as default) every implementation of Service&lt;,&gt; found in the assembly.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly"></param>
        /// <param name="lifeTime"></param>
        /// <returns></returns>
        public static IServiceCollection AddServices(this IServiceCollection services, Assembly assembly, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
        {
            return services.AddServices(lifeTime, assembly.GetServices());
        }

        /// <summary>
        /// Registers (as singleton instance, as default) the types.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="lifeTime"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static IServiceCollection AddServices(this IServiceCollection services, ServiceLifetime lifeTime = ServiceLifetime.Singleton, params Type[] types)
        {
            foreach (var type in types)
            {
                services.Add(new ServiceDescriptor(type, type, lifeTime));
            }

            return services;
        }
        
        /// <summary>
        /// Registers as singleton instance the types.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static IServiceCollection AddServices(this IServiceCollection services, params Type[] types)
        {
            foreach (var type in types)
            {
                services.AddSingleton(type);
            }

            return services;
        }

        /// <summary>
        /// Returns the implementations of Service&lt;,&gt; found in assembly in which the specified type is defined.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type[] GetAssemblyServices(this Type type)
        {
            var assembly = Assembly.GetAssembly(type);

            if (assembly == null)
            {
                return Array.Empty<Type>();
            }

            return assembly
                .GetExportedTypes()
                .Where(y => y.IsClass && !y.IsAbstract && !y.IsGenericType && !y.IsNested && y.IsService())
                .ToArray();
        }

        /// <summary>
        /// Returns the implementations of Service&lt;,&gt; found in the assembly.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static Type[] GetServices(this Assembly assembly)
        {
            return assembly
                .GetExportedTypes()
                .Where(t => t.IsService())
                .ToArray();
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
}