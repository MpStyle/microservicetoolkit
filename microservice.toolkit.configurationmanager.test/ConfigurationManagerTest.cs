using microservice.toolkit.configurationmanager.extension;

using Microsoft.Extensions.Configuration;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace microservice.toolkit.configurationmanager.test
{
    [ExcludeFromCodeCoverage]
    public class ConfigurationManagerTest
    {
        private IConfiguration configurationManager;

        [Test]
        public void GetString()
        {
            var stringValue = this.configurationManager.GetString("stringValue");
            Assert.AreEqual("Hello World!", stringValue);
        }

        [Test]
        public void GetString_DefaultValue()
        {
            const string defaultValue = "Ciao Mondo!";
            var stringValue = this.configurationManager.GetString("stringDefaultValue", defaultValue);
            Assert.AreEqual(defaultValue, stringValue);
        }

        [Test]
        public void GetInt()
        {
            var intValue = this.configurationManager.GetInt("intValue");
            Assert.AreEqual(666, intValue);
        }

        [Test]
        public void GetInt_Default()
        {
            var intValue = this.configurationManager.GetInt("intDefaultValue", 69);
            Assert.AreEqual(69, intValue);
        }

        [Test]
        public void GetBool()
        {
            var boolValue = this.configurationManager.GetBool("boolValue");
            Assert.IsTrue(boolValue);
        }

        [Test]
        public void GetBool_Default()
        {
            var boolValue = this.configurationManager.GetBool("boolDefaultValue", true);
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
        public void GetStringArray_Default()
        {
            var stringArrayValue =
                this.configurationManager.GetStringArray("stringArrayDefaultValue", new[] { "Ciao", "Mondo", "!" });
            Assert.AreEqual("Ciao", stringArrayValue[0]);
            Assert.AreEqual("Mondo", stringArrayValue[1]);
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

        [Test]
        public void GetIntArray_Default()
        {
            var intArrayValue = this.configurationManager.GetIntArray("intArrayDefaultValue", new[] { 0, 9, 8, 7 });
            Assert.AreEqual(0, intArrayValue[0]);
            Assert.AreEqual(9, intArrayValue[1]);
            Assert.AreEqual(8, intArrayValue[2]);
            Assert.AreEqual(7, intArrayValue[3]);
        }

        #region SetUp & TearDown

        [SetUp]
        public void SetUp()
        {
            this.configurationManager = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine("data", "ConfigurationManagerTest.json"))
                .Build();
        }

        [TearDown]
        public void TearDown()
        {
        }

        #endregion
    }
}