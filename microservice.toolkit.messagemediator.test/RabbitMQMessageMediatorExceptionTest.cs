using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.messagemediator.test;

[ExcludeFromCodeCoverage]
public class RabbitMQMessageMediatorExceptionTests
{
    [Test]
    public void Constructor_SetsErrorCode()
    {
        // Arrange
        const string expectedErrorCode = "42";

        // Act
        var ex = new RabbitMQMessageMediatorException(expectedErrorCode);

        // Assert
        Assert.That(ex.ErrorCode, Is.EqualTo(expectedErrorCode));
    }

    [Test]
    public void Inherits_FromException()
    {
        // Act
        var ex = new RabbitMQMessageMediatorException("1");

        // Assert
        Assert.That(ex, Is.InstanceOf<Exception>());
    }
}