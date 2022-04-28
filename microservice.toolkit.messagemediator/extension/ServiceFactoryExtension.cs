using microservice.toolkit.messagemediator.attribute;

using System;
using System.Collections.Generic;
using System.Linq;

namespace microservice.toolkit.messagemediator.extension;

public static class ServiceFactoryExtension
{
    public static ServiceFactory ByNameServiceFactory(this IServiceProvider serviceProvider, IEnumerable<Type> serviceTypes)
    {
        var mapper = new Dictionary<string, Type>();

        foreach (var serviceType in serviceTypes)
        {
            var attrs = Attribute.GetCustomAttributes(serviceType); 

            foreach (var attr in attrs)
            {
                if (attr is MicroService microService)
                {
                    mapper.Add(microService.Name, serviceType);
                }
            }
        }

        return pattern =>
        {
            var serviceType = mapper.FirstOrDefault(
                ms => ms.Key.Equals(pattern),
                new KeyValuePair<string, Type>("default", null)
            );

            return serviceProvider.GetService(serviceType.Value) as IService;
        };
    }
}