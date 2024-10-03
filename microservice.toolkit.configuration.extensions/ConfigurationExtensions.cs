using Microsoft.Extensions.Configuration;

using System;
using System.Diagnostics;

namespace microservice.toolkit.configuration.extensions;

public static class ConfigurationExtensions
{
    public static bool GetBool(this IConfiguration configuration, string key, bool defaultValue = default)
    {
        try
        {
            return bool.Parse(configuration[key]);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return defaultValue;
        }
    }

    public static int GetInt(this IConfiguration configuration, string key, int defaultValue = default)
    {
        try
        {
            return int.Parse(configuration[key]);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return defaultValue;
        }
    }

    public static string GetString(this IConfiguration configuration, string key, string defaultValue = default)
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

    public static string[] GetStringArray(this IConfiguration configuration, string key,
        string[] defaultValue = default)
    {
        try
        {
            var section = configuration.GetSection(key);
            return section.Get<string[]>() ?? defaultValue;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return defaultValue;
        }
    }

    public static int[] GetIntArray(this IConfiguration configuration, string key, int[] defaultValue = default)
    {
        try
        {
            var section = configuration.GetSection(key);
            return section.Get<int[]>() ?? defaultValue;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return defaultValue;
        }
    }
}