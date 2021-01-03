using System.IO;
using Microsoft.Extensions.Configuration;

using mpstyle.microservice.toolkit.book.configurationmanager;

using System;

using Xunit;

namespace microservicetoolkit.test.book.configurationmanager
{
    public class ConfigurationManagerTest : IDisposable
    {
        private readonly ConfigurationManager configurationManager;

        [Fact]
        public void GetString()
        {
            var stringValue = this.configurationManager.GetString("stringValue");
            Assert.Equal(stringValue, "Hello World!");
        }

        [Fact]
        public void GetInt()
        {
            var intValue = this.configurationManager.GetInt("intValue");
            Assert.Equal(intValue, 666);
        }

        [Fact]
        public void GetBool()
        {
            var boolValue = this.configurationManager.GetBool("boolValue");
            Assert.Equal(boolValue, true);
        }

        [Fact]
        public void GetStringArray()
        {
            var stringArrayValue = this.configurationManager.GetStringArray("stringArrayValue");
            Assert.Equal(stringArrayValue[0], "Hello");
            Assert.Equal(stringArrayValue[1], "World");
            Assert.Equal(stringArrayValue[2], "!");
        }

        [Fact]
        public void GetIntArray()
        {
            var intArrayValue = this.configurationManager.GetIntArray("intArrayValue");
            Assert.Equal(intArrayValue[0], 1);
            Assert.Equal(intArrayValue[1], 2);
            Assert.Equal(intArrayValue[2], 3);
            Assert.Equal(intArrayValue[3], 4);
        }

        #region SetUp & TearDown
        public ConfigurationManagerTest()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine("data", "ConfigurationManagerTest.json"))
                .Build();
            this.configurationManager = new ConfigurationManager(configuration, new DoNothingLogger<ConfigurationManager>());
        }

        public void Dispose()
        {
        }
        #endregion
    }
}