using NUnit.Framework;

using System;

namespace microservice.toolkit.messagemediator.test
{
    [TestFixture]
    public class InvalidServiceExceptionTests
    {
        [Test]
        public void Constructor_SetsServiceTypeFullName_MessageIsCorrect()
        {
            // Arrange
            const string serviceType = "MyNamespace.MyService";

            // Act
            var ex = new InvalidServiceException(serviceType);

            // Assert
            Assert.That(ex.Message, Is.EqualTo($"Service \"{serviceType}\" is not supported"));
        }

        [Test]
        public void Exception_IsSerializable()
        {
            // Arrange
            const string serviceType = "MyNamespace.MyService";
            var ex = new InvalidServiceException(serviceType);

            // Act & Assert
            Assert.That(ex, Is.InstanceOf<Exception>());
        }
    }
}