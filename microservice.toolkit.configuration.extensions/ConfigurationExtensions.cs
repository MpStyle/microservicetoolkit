using Microsoft.Extensions.Configuration;

using System;
using System.Diagnostics;

namespace microservice.toolkit.configuration.extensions;

public static class ConfigurationExtensions
{
    public static bool? GetNullableBool(this IConfiguration configuration, string key, bool? defaultValue = null)
    {
        try
        {
            var value = configuration[key];
            return string.IsNullOrEmpty(value) ? defaultValue : bool.Parse(value);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return defaultValue;
        }
    }

    public static bool GetBool(this IConfiguration configuration, string key, bool defaultValue = false)
    {
        try
        {
            return bool.Parse(configuration[key] ?? throw new InvalidOperationException());
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return defaultValue;
        }
    }

    public static int? GetNullableInt(this IConfiguration configuration, string key, int? defaultValue = null)
    {
        try
        {
            var value = configuration[key];
            return string.IsNullOrEmpty(value) ? defaultValue : int.Parse(value);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return defaultValue;
        }
    }

    public static int GetInt(this IConfiguration configuration, string key, int defaultValue = 0)
    {
        try
        {
            return int.Parse(configuration[key] ?? throw new InvalidOperationException());
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return defaultValue;
        }
    }

    public static string GetString(this IConfiguration configuration, string key, string defaultValue = null)
    {
        try
        {
            return configuration[key] ?? defaultValue;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return defaultValue;
        }
    }

    public static string[] GetStringArray(this IConfiguration configuration, string key, string[] defaultValue = null)
    {
        try
        {
            var section = configuration.GetSection(key);

            if (!section.Exists())
            {
                return defaultValue ?? Array.Empty<string>();
            }

            var arr = section.Get<string[]>();
            return arr;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return defaultValue;
        }
    }

    public static int[] GetIntArray(this IConfiguration configuration, string key, int[] defaultValue = null)
    {
        try
        {
            var section = configuration.GetSection(key);

            if (!section.Exists())
            {
                return defaultValue ?? Array.Empty<int>();
            }

            var arr = section.Get<int[]>();
            return arr;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return defaultValue;
        }
    }
}