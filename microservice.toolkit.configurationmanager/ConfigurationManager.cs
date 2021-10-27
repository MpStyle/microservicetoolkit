using microservice.toolkit.configurationmanager.extension;
using microservice.toolkit.core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;

namespace microservice.toolkit.configurationmanager
{
    [Obsolete("Use IConfigurationManager extensions instead. Import <i>microservice.toolkit.configurationmanager.extension</i> namespace.", false)]
    public class ConfigurationManager : IConfigurationManager
    {
        private readonly IConfiguration configuration;

        public ConfigurationManager(IConfiguration configuration, ILogger<ConfigurationManager> _)
        {
            this.configuration = configuration;
        }

        public ConfigurationManager(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public bool GetBool(string key)
        {
            return this.configuration.GetBool(key);
        }

        public int GetInt(string key)
        {
            return this.configuration.GetInt(key);
        }

        public string GetString(string key)
        {
            return this.configuration.GetString(key);
        }

        public string[] GetStringArray(string key)
        {
            return this.configuration.GetStringArray(key);
        }

        public int[] GetIntArray(string key)
        {
            return this.configuration.GetIntArray(key);
        }
    }
}