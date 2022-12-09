using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.tsid.test;

[ExcludeFromCodeCoverage]
public class TsidCreatorTest
{
    private static readonly int LOOP_MAX = 1_000;

    [Test]
    public void Tsid1024()
    {
        var tsid = TsidCreator.Tsid1024();
        Assert.IsNotNull(tsid.ToString());
    }

    [Test]
    public void TestGetInstant()
    {
        DateTimeOffset start = DateTimeOffset.Now;
        Tsid tsid = (new TsidFactory()).Create();
        DateTimeOffset middle = tsid.GetInstant();
        DateTimeOffset end = DateTimeOffset.Now;

        Assert.IsTrue(start.ToUnixTimeMilliseconds() <= middle.ToUnixTimeMilliseconds());
        Assert.IsTrue(middle.ToUnixTimeMilliseconds() <= end.ToUnixTimeMilliseconds());
    }

    [Test]
    public void TestGetUnixMilliseconds()
    {
        long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        Tsid tsid = (new TsidFactory()).Create();
        long middle = tsid.GetUnixMilliseconds();
        long end = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        Assert.IsTrue(start <= middle);
        Assert.IsTrue(middle <= end);
    }

    [Test]
    public void testGetInstantWithClock()
    {
        var bound = (long)Math.Pow(2, 42);

        for (int i = 0; i < LOOP_MAX; i++)
        {
            // instantiate a factory with a Clock that returns a fixed value
            var random = this.LongRandom();
            var millis = random + Tsid.TsidEpoch; // avoid dates before 2020
            var clock = DateTimeOffset.FromUnixTimeMilliseconds(millis); // simulate a frozen clock
            var randomFunction = (int x) => new byte[x]; // force to reinitialize the counter to ZERO
            var factory = new TsidFactory.Builder().WithClock(clock).WithRandomFunction(randomFunction).Build();
            var result = factory.Create().GetInstant().ToUnixTimeMilliseconds();

            Assert.AreEqual(millis, result, "The current instant is incorrect");
        }
    }

    [Test]
    public void testGetUnixMillisecondsWithClock()
    {
        var bound = (long)Math.Pow(2, 42);

        for (int i = 0; i < LOOP_MAX; i++)
        {
            // instantiate a factory with a Clock that returns a fixed value
            var random = this.LongRandom();
            var millis = random + Tsid.TsidEpoch; // avoid dates before 2020
            var clock = DateTimeOffset.FromUnixTimeMilliseconds(millis); // simulate a frozen clock
            var randomFunction = (int x) => new byte[x]; // force to reinitialize the counter to ZERO
            var factory = new TsidFactory.Builder().WithClock(clock).WithRandomFunction(randomFunction).Build();
            var result = factory.Create().GetUnixMilliseconds();

            Assert.AreEqual(millis, result, "The current millisecond is incorrect");
        }
    }

    [Test]
    public void testGetInstantWithCustomEpoch()
    {
        var customEpoch = DateTimeOffset.Parse("2015-10-23T00:00:00Z");

        var start = DateTimeOffset.Now;
        var tsid = new TsidFactory.Builder().WithCustomEpoch(customEpoch).Build().Create();
        var middle = tsid.GetInstant(customEpoch);
        var end = DateTimeOffset.Now;

        Assert.IsTrue(start.ToUnixTimeMilliseconds() <= middle.ToUnixTimeMilliseconds());
        Assert.IsTrue(middle.ToUnixTimeMilliseconds() <= end.ToUnixTimeMilliseconds());
    }

    [Test]
    public void testGetUnixMillisecondsWithCustomEpoch()
    {
        var customEpoch = DateTimeOffset.Parse("1984-01-01T00:00:00Z");
        var start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var tsid = new TsidFactory.Builder().WithCustomEpoch(customEpoch).Build().Create();
        var middle = tsid.GetInstant(customEpoch).ToUnixTimeMilliseconds();
        var end = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Assert.IsTrue(start <= middle);
        Assert.IsTrue(middle <= end);
    }

    [Test]
    public void testWithNode()
    {
        {
            for (var i = 0; i <= 20; i++)
            {
                var bits = TsidFactory.NodeBits1024;
                var shif = Tsid.RandomBits - bits;
                var mask = (1 << bits - 1);
                var node = this.IntRandom() & mask;
                var factory = new TsidFactory(node);
                Assert.AreEqual(node, (factory.Create().GetRandom() >>> shif) & mask);
            }
        }
        {
            for (var i = 0; i <= 20; i++)
            {
                var bits = TsidFactory.NodeBits1024;
                var shif = Tsid.RandomBits - bits;
                var mask = (1 << bits - 1);
                var node = this.IntRandom() & mask;
                var factory = new TsidFactory.Builder().WithNode(node).Build();
                Assert.AreEqual(node, (factory.Create().GetRandom() >>> shif) & mask);
            }
        }
        {
            for (var i = 0; i <= 20; i++)
            {
                var bits = TsidFactory.NodeBits256;
                var shif = Tsid.RandomBits - bits;
                var mask = (1 << bits - 1);
                var node = this.IntRandom() & mask;
                var factory = TsidFactory.NewInstance256(node);
                Assert.AreEqual(node, (factory.Create().GetRandom() >>> shif) & mask);
            }
        }
        {
            for (var i = 0; i <= 20; i++)
            {
                var bits = TsidFactory.NodeBits1024;
                var shif = Tsid.RandomBits - bits;
                var mask = (1 << bits - 1);
                var node = this.IntRandom() & mask;
                var factory = TsidFactory.NewInstance1024(node);
                Assert.AreEqual(node, (factory.Create().GetRandom() >>> shif) & mask);
            }
        }
        {
            for (var i = 0; i <= 20; i++)
            {
                var bits = TsidFactory.NodeBits4096;
                var shif = Tsid.RandomBits - bits;
                var mask = (1 << bits - 1);
                var node = this.IntRandom() & mask;
                var factory = TsidFactory.NewInstance4096(node);
                Assert.AreEqual(node, (factory.Create().GetRandom() >>> shif) & mask);
            }
        }
    }

    private long LongRandom(long min = 10000, long max = 90000)
    {
        var rand = new Random();
        var result = rand.Next((Int32)(min >> 32), (Int32)(max >> 32));
        result = (result << 32);
        result = result | (long)rand.Next((Int32)min, (Int32)max);
        return result;
    }

    private int IntRandom(int min = 100, int max = 90000)
    {
        var rand = new Random();
        return rand.Next(min, max);
    }
}
