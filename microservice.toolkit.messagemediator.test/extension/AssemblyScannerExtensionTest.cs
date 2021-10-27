using microservice.toolkit.messagemediator.extension;
using microservice.toolkit.messagemediator.test.data;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace microservice.toolkit.messagemediator.test.extension
{
    [ExcludeFromCodeCoverage]
    public class AssemblyScannerExtensionTest
    {
        [Test]
        public void GetServices()
        {
            var serviceTypes = Assembly.GetAssembly(typeof(ValidService01)).GetServices();
            Assert.AreEqual(2, serviceTypes.Length);
            Assert.AreEqual(typeof(ValidService01).FullName, serviceTypes[0].FullName);
            Assert.AreEqual(typeof(ValidService02).FullName, serviceTypes[1].FullName);
        }

        [Test]
        public void GetAssemblyServices()
        {
            var serviceTypes = typeof(ValidService01).GetAssemblyServices();
            Assert.AreEqual(2, serviceTypes.Length);
            Assert.AreEqual(typeof(ValidService01).FullName, serviceTypes[0].FullName);
            Assert.AreEqual(typeof(ValidService02).FullName, serviceTypes[1].FullName);
        }
    }
}