using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.messagemediator.test;

[ExcludeFromCodeCoverage]
public class SignalEmitterExceptionTests
{
    [Test]
    public void Constructor_SetsErrorCodeInMessage()
    {
        // Arrange
        var errorCode = 42;

        // Act
        var ex = new SignalEmitterException(errorCode);

        // Assert (Constraint Model)
        Assert.That(ex, Is.InstanceOf<SignalEmitterException>());
        Assert.That(ex.Message, Is.EqualTo($"SignalEmitter error code: {errorCode}"));
    }
}