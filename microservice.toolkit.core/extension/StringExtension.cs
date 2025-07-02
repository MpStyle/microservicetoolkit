using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.core.extension;

public static class StringExtension
{
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? s)
    {
        return string.IsNullOrEmpty(s);
    }

    public static bool IsNotNullOrEmpty(this string? s)
    {
        return !string.IsNullOrEmpty(s);
    }
}