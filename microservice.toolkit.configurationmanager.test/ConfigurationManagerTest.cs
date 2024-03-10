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
            Assert.That("Hello World!", Is.EqualTo(stringValue));
        }

        [Test]
        public void GetString_DefaultValue()
        {
            const string defaultValue = "Ciao Mondo!";
            var stringValue = this.configurationManager.GetString("stringDefaultValue", defaultValue);
            Assert.That(defaultValue, Is.EqualTo(stringValue));
        }

        [Test]
        public void GetInt()
        {
            var intValue = this.configurationManager.GetInt("intValue");
            Assert.That(666, Is.EqualTo(intValue));
        }

        [Test]
        public void GetInt_Default()
        {
            var intValue = this.configurationManager.GetInt("intDefaultValue", 69);
            Assert.That(69, Is.EqualTo(intValue));
        }

        [Test]
        public void GetBool()
        {
            var boolValue = this.configurationManager.GetBool("boolValue");
            Assert.That(boolValue, Is.True);
        }

        [Test]
        public void GetBool_Default()
        {
            var boolValue = this.configurationManager.GetBool("boolDefaultValue", true);
            Assert.That(boolValue, Is.True);
        }

        [Test]
        public void GetStringArray()
        {
            var stringArrayValue = this.configurationManager.GetStringArray("stringArrayValue");
            Assert.That("Hello", Is.EqualTo(stringArrayValue[0]));
            Assert.That("World", Is.EqualTo(stringArrayValue[1]));
            Assert.That("!", Is.EqualTo(stringArrayValue[2]));
        }

        [Test]
        public void GetStringArray_Default()
        {
            var stringArrayValue =
                this.configurationManager.GetStringArray("stringArrayDefaultValue", ["Ciao", "Mondo", "!"]);
            Assert.That("Ciao", Is.EqualTo(stringArrayValue[0]));
            Assert.That("Mondo", Is.EqualTo(stringArrayValue[1]));
            Assert.That("!", Is.EqualTo(stringArrayValue[2]));
        }

        [Test]
        public void GetIntArray()
        {
            var intArrayValue = this.configurationManager.GetIntArray("intArrayValue");
            Assert.That(1, Is.EqualTo(intArrayValue[0]));
            Assert.That(2, Is.EqualTo(intArrayValue[1]));
            Assert.That(3, Is.EqualTo(intArrayValue[2]));
            Assert.That(4, Is.EqualTo(intArrayValue[3]));
        }

        [Test]
        public void GetIntArray_Default()
        {
            var intArrayValue = this.configurationManager.GetIntArray("intArrayDefaultValue", [0, 9, 8, 7]);
            Assert.That(0, Is.EqualTo(intArrayValue[0]));
            Assert.That(9, Is.EqualTo(intArrayValue[1]));
            Assert.That(8, Is.EqualTo(intArrayValue[2]));
            Assert.That(7, Is.EqualTo(intArrayValue[3]));
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