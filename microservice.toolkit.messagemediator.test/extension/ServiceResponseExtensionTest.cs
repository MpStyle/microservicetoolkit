using microservice.toolkit.core.entity;
using microservice.toolkit.core.extension;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.core.test.extension;

[ExcludeFromCodeCoverage]
public class ServiceResponseExtensionTest
{
    [Test]
    public void IsSuccessful_ShouldReturnTrue_WhenErrorIsNull()
    {
        // Arrange
        var serviceResponse = new ServiceResponse<string> { Payload = "Success", Error = null };
        // Act
        var result = serviceResponse.IsSuccessful();
        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsSuccessful_ShouldReturnFalse_WhenErrorIsNotNull()
    {
        // Arrange
        var serviceResponse = new ServiceResponse<string> { Payload = "Failure", Error = 1 };
        // Act
        var result = serviceResponse.IsSuccessful();
        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsError_ShouldReturnTrue_WhenErrorIsNotNull()
    {
        // Arrange
        var serviceResponse = new ServiceResponse<string> { Payload = "Failure", Error = 1 };
        // Act
        var result = serviceResponse.IsError();
        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsError_ShouldReturnFalse_WhenErrorIsNull()
    {
        // Arrange
        var serviceResponse = new ServiceResponse<string> { Payload = "Success", Error = null };
        // Act
        var result = serviceResponse.IsError();
        // Assert
        Assert.That(result, Is.False);
    }
}
