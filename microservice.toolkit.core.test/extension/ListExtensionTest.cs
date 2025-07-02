using microservice.toolkit.core.extension;

using NUnit.Framework;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.core.test.extension;

[ExcludeFromCodeCoverage]
public class ListExtensionTest
{
    [Test]
    public void IsNullOrEmpty_NullList_ReturnsTrue()
    {
        List<int>? l = null;
        Assert.That(l.IsNullOrEmpty(), Is.True);
    }
    
    
    [Test]
    public void IsNullOrEmpty_EmptyList_ReturnsTrue()
    {
        var l = new List<int>();
        Assert.That(l.IsNullOrEmpty(), Is.True);
    }
}