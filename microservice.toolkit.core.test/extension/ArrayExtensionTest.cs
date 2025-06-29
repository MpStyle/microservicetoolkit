using microservice.toolkit.core.extension;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.core.test.extension;

[ExcludeFromCodeCoverage]
public class ArrayExtensionTest
{
    [Test]
    public void IsNullOrEmpty_NullArray_ReturnsTrue()
    {
        int[]? array = null;
        Assert.That(array.IsNullOrEmpty(), Is.True);
    }
    
    [Test]
    public void IsNullOrEmpty_EmptyArray_ReturnsTrue()
    {
        var array = Array.Empty<int>();
        Assert.That(array.IsNullOrEmpty(), Is.True);
    }
    
    [Test]
    public void IsNullOrEmpty_NonEmptyArray_ReturnsFalse()
    {
        var array = new[] { 1, 2, 3 };
        Assert.That(array.IsNullOrEmpty(), Is.False);
    }
    
    [Test]
    public void ConcatArrays_NullArray_ThrowsArgumentNullException()
    {
        int[]? array = null;
        Assert.Throws<ArgumentNullException>(() => array.ConcatArrays([1, 2, 3]));
    }
    
    [Test]
    public void ConcatArrays_NullParams_ReturnsOriginalArray()
    {
        var array = new[] { 1, 2, 3 };
        var result = array.ConcatArrays(null);
        Assert.That(result, Is.EqualTo(array));
    }
    
    [Test]
    public void ConcatArrays_ValidParams_ReturnsConcatenatedArray()
    {
        var array = new[] { 1, 2, 3 };
        var result = array.ConcatArrays([4, 5], [6]);
        Assert.That(result, Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6 }));
    }
}