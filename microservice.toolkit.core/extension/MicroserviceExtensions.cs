using microservice.toolkit.core.attribute;

using System;
using System.Diagnostics;
using System.Linq;

namespace microservice.toolkit.core.extension;

public static class MicroserviceExtensions
{
    public static string ToPattern(this Type type)
    {
        var attrs = Attribute.GetCustomAttributes(type).FirstOrDefault(x => x is Microservice) as Microservice;

        if (attrs?.Pattern.IsNullOrEmpty() == false)
        {
            return attrs.Pattern;
        }

        Debug.Assert(type.FullName != null, "type.FullName != null");
        
        return type.FullName.Replace(".", "/");
    }
}
