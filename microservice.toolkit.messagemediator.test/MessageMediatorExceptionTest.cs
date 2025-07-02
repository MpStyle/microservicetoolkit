using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.messagemediator.test;

[ExcludeFromCodeCoverage]
public class MessageMediatorExceptionTest
{
    [Test]
    public void Constructor_SetsErrorCode()
    {
        var exception = new MessageMediatorException("42");

        Assert.That(exception.ErrorCode, Is.EqualTo("42"));
    }

    [Test]
    public void Message_ReturnsExpectedFormat()
    {
        var exception = new MessageMediatorException("99");

        Assert.That(exception.Message, Is.EqualTo("MessageMediator error code: 99"));
    }

    [Test]
    public void InheritsFromException()
    {
        var exception = new MessageMediatorException("1");

        Assert.That(exception, Is.InstanceOf<Exception>());
    }
}