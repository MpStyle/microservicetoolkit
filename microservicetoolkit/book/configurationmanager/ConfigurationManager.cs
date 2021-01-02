using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;

namespace mpstyle.microservice.toolkit.book.messagemediator
{
    public class ConfigurationManager : IConfigurationManager
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<ConfigurationManager> logger;

        public ConfigurationManager(IConfiguration configuration, ILogger<ConfigurationManager> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public bool GetBool(string key)
        {
            try
            {
                return bool.Parse(this.configuration[key]);
            }
            catch (Exception ex)
            {
                this.logger.LogDebug(ex, $"{this.GetType()} parsing boolean configuration \"{key}\"");
                return default;
            }
        }

        public int GetInt(string key)
        {
            try
            {
                return int.Parse(this.configuration[key]);
            }
            catch (Exception ex)
            {
                this.logger.LogDebug(ex, $"{this.GetType()} parsing integer configuration \"{key}\"");
                return default;
            }
        }

        public string GetString(string key)
        {
            try
            {
                return this.configuration[key];
            }
            catch (Exception ex)
            {
                this.logger.LogDebug(ex, $"{this.GetType()} parsing string configuration \"{key}\"");
                return default;
            }
        }
    }
}