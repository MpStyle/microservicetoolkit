using Microsoft.Extensions.Configuration;

using mpstyle.microservice.toolkit.book.configurationmanager;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace mpstyle.microservice.toolkit.test.book.configurationmanager
{
    [ExcludeFromCodeCoverage]
    public class ConfigurationManagerTest
    {
        private ConfigurationManager configurationManager;

        [Test]
        public void GetString()
        {
            var stringValue = this.configurationManager.GetString("stringValue");
            Assert.AreEqual("Hello World!", stringValue);
        }

        [Test]
        public void GetInt()
        {
            var intValue = this.configurationManager.GetInt("intValue");
            Assert.AreEqual(666, intValue);
        }

        [Test]
        public void GetBool()
        {
            var boolValue = this.configurationManager.GetBool("boolValue");
            Assert.IsTrue(boolValue);
        }

        [Test]
        public void GetStringArray()
        {
            var stringArrayValue = this.configurationManager.GetStringArray("stringArrayValue");
            Assert.AreEqual("Hello", stringArrayValue[0]);
            Assert.AreEqual("World", stringArrayValue[1]);
            Assert.AreEqual("!", stringArrayValue[2]);
        }

        [Test]
        public void GetIntArray()
        {
            var intArrayValue = this.configurationManager.GetIntArray("intArrayValue");
            Assert.AreEqual(1, intArrayValue[0]);
            Assert.AreEqual(2, intArrayValue[1]);
            Assert.AreEqual(3, intArrayValue[2]);
            Assert.AreEqual(4, intArrayValue[3]);
        }

        #region SetUp & TearDown
        [SetUp]
        public void SetUp()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine("data", "ConfigurationManagerTest.json"))
                .Build();
            this.configurationManager = new ConfigurationManager(configuration, new DoNothingLogger<ConfigurationManager>());
        }

        [TearDown]
        public void TearDown()
        {
        }
        #endregion
    }
}