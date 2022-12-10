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

        Assert.IsTrue(64 >= Convert.ToString(tsid.Number, 2).Length);
        Assert.AreEqual(13, tsid.ToString().Length);

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

        Assert.IsTrue(64 >= Convert.ToString(first.Number, 2).Length);
        Assert.AreEqual(13, first.ToString().Length);

        Assert.IsTrue(64 >= Convert.ToString(second.Number, 2).Length);
        Assert.AreEqual(13, second.ToString().Length);

        Assert.IsTrue(64 >= Convert.ToString(third.Number, 2).Length);
        Assert.AreEqual(13, third.ToString().Length);

        Assert.IsTrue(64 >= Convert.ToString(fourth.Number, 2).Length);
        Assert.AreEqual(13, fourth.ToString().Length);

        Assert.IsTrue(64 >= Convert.ToString(fifth.Number, 2).Length);
        Assert.AreEqual(13, fifth.ToString().Length);

        Assert.IsTrue(first.Number < second.Number);
        Assert.IsTrue(second.Number < third.Number);
        Assert.IsTrue(third.Number < fourth.Number);
        Assert.IsTrue(fourth.Number < fifth.Number);
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

        Assert.IsTrue(64 >= Convert.ToString(tsid.Number, 2).Length);
        Assert.AreEqual(13, tsid.ToString().Length);

        Assert.IsNotNull(tsid.Number);
        Assert.IsNotNull(tsid.ToString());
        Assert.IsNotNull(tsid.ToLowerCase());
        Assert.IsTrue(string.Equals(tsid.ToString(), tsid.ToLowerCase(), StringComparison.OrdinalIgnoreCase));
        Assert.IsNotNull(tsid.ToBytes());
    }
}
