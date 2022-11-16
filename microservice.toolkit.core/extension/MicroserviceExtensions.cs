using microservice.toolkit.messagemediator.attribute;

using System;
using System.Linq;

namespace microservice.toolkit.core.extension;

public static partial class MicroserviceExtensions
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
}
