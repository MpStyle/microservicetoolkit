using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.tsid.test;

[ExcludeFromCodeCoverage]
public class TsidCreatorTest
{
    [Test]
    public void Tsid1024()
    {
        var tsid = TsidCreator.Tsid1024();
        Assert.IsNotNull(tsid);
    }
}
