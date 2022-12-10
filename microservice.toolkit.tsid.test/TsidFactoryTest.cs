using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace microservice.toolkit.tsid.test;

[ExcludeFromCodeCoverage]
public class TsidFactoryTest
{
    [Test]
    public void Create_ChecksTsidParts()
    {
        const long sourceNode = 20;

        var start = DateTimeOffset.UtcNow;

        var factory = new TsidFactory(new TsidSettings
        {
            Node = sourceNode
        });
        var tsid = factory.Create();

        var end = DateTimeOffset.UtcNow;

        var time = tsid.GetTime();
        var datetime = DateTimeOffset.FromUnixTimeMilliseconds(TsidProps.TsidTimeEpoch + time);

        var node = tsid.GetNode();
        var sequence = tsid.GetSequence();

        Assert.AreEqual(sequence, 0);
        Assert.AreEqual(sourceNode, node);
        Assert.IsTrue(datetime.ToUnixTimeMilliseconds() >= start.ToUnixTimeMilliseconds(), $"{datetime.ToUnixTimeMilliseconds()}<{start.ToUnixTimeMilliseconds()}");
        Assert.IsTrue(datetime.ToUnixTimeMilliseconds() <= end.ToUnixTimeMilliseconds(), $"{datetime.ToUnixTimeMilliseconds()}>{end.ToUnixTimeMilliseconds()}");
    }

    [Test]
    public void Create_ChecksIncrementalByDesign()
    {
        const long sourceNode = 20;
        var factory = new TsidFactory(new TsidSettings
        {
            Node = sourceNode
        });
        var first = factory.Create();
        Thread.Sleep(1);
        var second = factory.Create();
        Thread.Sleep(1);
        var third = factory.Create();
        Thread.Sleep(1);
        var fourth = factory.Create();
        Thread.Sleep(1);
        var fifth = factory.Create();

        Assert.IsTrue(first.Number <= second.Number);
        Assert.IsTrue(second.Number <= third.Number);
        Assert.IsTrue(third.Number <= fourth.Number);
        Assert.IsTrue(fourth.Number <= fifth.Number);
    }

    [Test]
    public void Create_ChecksTsid()
    {
        const long sourceNode = 20;
        var factory = new TsidFactory(new TsidSettings
        {
            Node = sourceNode
        });
        var tsid = factory.Create();

        Assert.IsNotNull(tsid.Number);
        Assert.IsNotNull(tsid.ToString());
        Assert.IsNotNull(tsid.ToLowerCase());
        Assert.IsTrue(string.Equals(tsid.ToString(), tsid.ToLowerCase(), StringComparison.OrdinalIgnoreCase));
        Assert.IsNotNull(tsid.ToBytes());
    }
}
