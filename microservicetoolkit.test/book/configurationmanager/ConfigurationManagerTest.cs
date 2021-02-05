using System.IO;
using Microsoft.Extensions.Configuration;

using mpstyle.microservice.toolkit.book.configurationmanager;

using System;

using Xunit;

namespace mpstyle.microservice.toolkit.test.book.configurationmanager
{
    public class ConfigurationManagerTest : IDisposable
    {
        private readonly ConfigurationManager configurationManager;

        [Fact]
        public void GetString()
        {
            var stringValue = this.configurationManager.GetString("stringValue");
            Assert.Equal("Hello World!", stringValue);
        }

        [Fact]
        public void GetInt()
        {
            var intValue = this.configurationManager.GetInt("intValue");
            Assert.Equal(666, intValue);
        }

        [Fact]
        public void GetBool()
        {
            var boolValue = this.configurationManager.GetBool("boolValue");
            Assert.True(boolValue);
        }

        [Fact]
        public void GetStringArray()
        {
            var stringArrayValue = this.configurationManager.GetStringArray("stringArrayValue");
            Assert.Equal("Hello", stringArrayValue[0]);
            Assert.Equal("World", stringArrayValue[1]);
            Assert.Equal("!", stringArrayValue[2]);
        }

        [Fact]
        public void GetIntArray()
        {
            var intArrayValue = this.configurationManager.GetIntArray("intArrayValue");
            Assert.Equal(1,intArrayValue[0]);
            Assert.Equal(2,intArrayValue[1]);
            Assert.Equal(3,intArrayValue[2]);
            Assert.Equal(4,intArrayValue[3]);
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