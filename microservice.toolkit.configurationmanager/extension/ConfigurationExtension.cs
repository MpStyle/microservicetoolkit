using Microsoft.Extensions.Configuration;

using System;

namespace microservice.toolkit.configurationmanager.extension
{
    public static class ConfigurationExtension
    {
        public static bool GetBool(this IConfiguration configuration, string key)
        {
            try
            {
                return bool.Parse(configuration[key]);
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public static int GetInt(this IConfiguration configuration, string key)
        {
            try
            {
                return int.Parse(configuration[key]);
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public static string GetString(this IConfiguration configuration, string key)
        {
            try
            {
                return configuration[key];
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public static string[] GetStringArray(this IConfiguration configuration, string key)
        {
            try
            {
                var section = configuration.GetSection(key);
                return section.Get<string[]>();
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public static int[] GetIntArray(this IConfiguration configuration, string key)
        {
            try
            {
                var section = configuration.GetSection(key);
                return section.Get<int[]>();
            }
            catch (Exception ex)
            {
                return default;
            }
        }
    }
}