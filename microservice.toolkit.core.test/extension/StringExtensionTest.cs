using microservice.toolkit.core.extension;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.core.test.extension;

[ExcludeFromCodeCoverage]
public class StringExtensionTest
{
    [Test]
    public void IsNullOrEmpty_ShouldReturnTrue_WhenStringIsNull()
    {
        string? str = null;
        var result = str.IsNullOrEmpty();
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void IsNullOrEmpty_ShouldReturnTrue_WhenStringIsEmpty()
    {
        var str = string.Empty;
        var result = str.IsNullOrEmpty();
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void IsNullOrEmpty_ShouldReturnFalse_WhenStringIsNotEmpty()
    {
        const string str = "test";
        var result = str.IsNullOrEmpty();
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void IsNotNullOrEmpty_ShouldReturnFalse_WhenStringIsNull()
    {
        string? str = null;
        var result = str.IsNotNullOrEmpty();
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void IsNotNullOrEmpty_ShouldReturnFalse_WhenStringIsEmpty()
    {
        var str = string.Empty;
        var result = str.IsNotNullOrEmpty();
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void IsNotNullOrEmpty_ShouldReturnTrue_WhenStringIsNotEmpty()
    {
        const string str = "test";
        var result = str.IsNotNullOrEmpty();
        Assert.That(result, Is.True);
    }
}