using microservice.toolkit.core.attribute;

using System;
using System.Diagnostics;
using System.Linq;

namespace microservice.toolkit.core.extension;

public static class MicroserviceExtensions
{
    public static string ToPattern(this Type type)
    {
        var attrs = Attribute.GetCustomAttributes(type).OfType<Microservice>().FirstOrDefault();

        if (attrs?.Pattern.IsNullOrEmpty() == false)
        {
            return attrs.Pattern;
        }

        Debug.Assert(type.FullName != null, "type.FullName != null");

        return type.FullName.Replace(".", "/");
    }

    public static string[] ToPatterns(this Type type)
    {
        var patterns = Attribute.GetCustomAttributes(type).OfType<Microservice>()
            .Where(a => a.Pattern.IsNullOrEmpty() == false)
            .Select(a => a.Pattern)
            .ToArray();

        if (patterns.Any())
        {
            return patterns;
        }

        Debug.Assert(type.FullName != null, "type.FullName != null");

        return [type.FullName.Replace(".", "/")];
    }
}