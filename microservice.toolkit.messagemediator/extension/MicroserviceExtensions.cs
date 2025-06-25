using microservice.toolkit.messagemediator.attribute;

using System;
using System.Diagnostics;
using System.Linq;

namespace microservice.toolkit.messagemediator.extension;

/// <summary>
/// Extension methods for extracting message patterns from Microservice attributes.
/// </summary>
public static class MicroserviceExtensions
{
    /// <summary>
    /// Returns the first non-empty pattern from the Microservice attribute, or the type's full name as a fallback.
    /// </summary>
    public static string ToPattern(this Type type)
    {
        var attr = Attribute.GetCustomAttributes(type)
            .OfType<Microservice>()
            .FirstOrDefault(a => !string.IsNullOrEmpty(a.Pattern));

        if (attr is { Pattern: { } pattern } && !string.IsNullOrEmpty(pattern))
        {
            return pattern;
        }

        Debug.Assert(type.FullName != null, "type.FullName != null");
        return type.FullName?.Replace('.', '/') ?? string.Empty;
    }

    /// <summary>
    /// Returns all non-empty patterns from Microservice attributes, or the type's full name as a fallback.
    /// </summary>
    public static string[] ToPatterns(this Type type)
    {
        var patterns = Attribute.GetCustomAttributes(type)
            .OfType<Microservice>()
            .Select(a => a.Pattern)
            .Where(p => !string.IsNullOrEmpty(p))
            .ToArray();

        if (patterns.Length > 0)
        {
            return patterns;
        }

        Debug.Assert(type.FullName != null, "type.FullName != null");
        return [type.FullName?.Replace('.', '/') ?? string.Empty];
    }
}