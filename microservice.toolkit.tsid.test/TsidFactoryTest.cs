using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace microservice.toolkit.tsid.test;

[ExcludeFromCodeCoverage]
public class TsidFactoryTest
{
    [Test]
    public void CheckNativeSort_Number_1024()
    {
        var tsids = new long[1000];
        var factory = new TsidFactory();

        for (var i = 0; i < tsids.Length; i++)
        {
            tsids[i] = factory.Create().Number;
        }
        var sortedTsids = Sort(tsids);

        Assert.AreEqual(tsids.Length, sortedTsids.Length);
        for (var i = 0; i < tsids.Length; i++)
        {
            Assert.AreEqual(tsids[i], sortedTsids[i]);
        }
    }

    [Test]
    public void CheckNativeSort_Number_256()
    {
        var tsids = new long[1000];
        var factory = new TsidFactory(new TsidSettings
        {
            TsidLength = TsidLength.Tsid256
        });

        for (var i = 0; i < tsids.Length; i++)
        {
            tsids[i] = factory.Create().Number;
        }
        var sortedTsids = Sort(tsids);

        Assert.AreEqual(tsids.Length, sortedTsids.Length);
        for (var i = 0; i < tsids.Length; i++)
        {
            Assert.AreEqual(tsids[i], sortedTsids[i]);
        }
    }

    [Test]
    public void CheckNativeSort_Number_4096()
    {
        var tsids = new long[1000];
        var factory = new TsidFactory(new TsidSettings
        {
            TsidLength = TsidLength.Tsid4096
        });

        for (var i = 0; i < tsids.Length; i++)
        {
            tsids[i] = factory.Create().Number;
        }
        var sortedTsids = Sort(tsids);

        Assert.AreEqual(tsids.Length, sortedTsids.Length);
        for (var i = 0; i < tsids.Length; i++)
        {
            Assert.AreEqual(tsids[i], sortedTsids[i]);
        }
    }

    [Test]
    public void CheckNativeSort_String_1024()
    {
        var tsids = new string[1000];
        var factory = new TsidFactory();

        for (var i = 0; i < tsids.Length; i++)
        {
            tsids[i] = factory.Create().ToString();
        }
        var sortedTsids = Sort(tsids);

        Assert.AreEqual(tsids.Length, sortedTsids.Length);
        for (var i = 0; i < tsids.Length; i++)
        {
            Assert.AreEqual(tsids[i], sortedTsids[i]);
        }
    }

    [Test]
    public void CheckNativeSort_String_256()
    {
        var tsids = new string[1000];
        var factory = new TsidFactory(new TsidSettings
        {
            TsidLength = TsidLength.Tsid256
        });

        for (var i = 0; i < tsids.Length; i++)
        {
            tsids[i] = factory.Create().ToString();
        }
        var sortedTsids = Sort(tsids);

        Assert.AreEqual(tsids.Length, sortedTsids.Length);
        for (var i = 0; i < tsids.Length; i++)
        {
            Assert.AreEqual(tsids[i], sortedTsids[i]);
        }
    }

    [Test]
    public void CheckNativeSort_String_4096()
    {
        var tsids = new string[1000];
        var factory = new TsidFactory(new TsidSettings
        {
            TsidLength = TsidLength.Tsid4096
        });

        for (var i = 0; i < tsids.Length; i++)
        {
            tsids[i] = factory.Create().ToString();
        }
        var sortedTsids = Sort(tsids);

        Assert.AreEqual(tsids.Length, sortedTsids.Length);
        for (var i = 0; i < tsids.Length; i++)
        {
            Assert.AreEqual(tsids[i], sortedTsids[i]);
        }
    }

    [Test]
    public void Create_CustomTsidTimeEpoch()
    {
        const long sourceNode = 20;
        var start = DateTimeOffset.UtcNow;
        var factory = new TsidFactory(new TsidSettings
        {
            Node = sourceNode,
            TsidLength = TsidLength.Tsid256,
            CustomTsidTimeEpoch = DateTimeOffset.UtcNow
        });
        var tsid = factory.Create();
        var end = DateTimeOffset.UtcNow;

        var time = GetTime(tsid);
        var datetime = DateTimeOffset.FromUnixTimeMilliseconds(factory.TsidTimeEpoch.ToUnixTimeMilliseconds() + time);

        var node = GetNode(tsid, 8);
        var sequence = GetSequence(tsid, 14);

        Assert.IsTrue(64 >= Convert.ToString(tsid.Number, 2).Length);
        Assert.AreEqual(13, tsid.ToString().Length);

        Assert.AreEqual(sequence, 0);
        Assert.AreEqual(sourceNode, node);
        Assert.IsTrue(datetime.ToUnixTimeMilliseconds() >= start.ToUnixTimeMilliseconds(), $"{datetime.ToUnixTimeMilliseconds()}<{start.ToUnixTimeMilliseconds()}");
        Assert.IsTrue(datetime.ToUnixTimeMilliseconds() <= end.ToUnixTimeMilliseconds(), $"{datetime.ToUnixTimeMilliseconds()}>{end.ToUnixTimeMilliseconds()}");
    }

    [Test]
    public void Create_Tsid256()
    {
        const long sourceNode = 20;
        var start = DateTimeOffset.UtcNow;
        var factory = new TsidFactory(new TsidSettings
        {
            Node = sourceNode,
            TsidLength = TsidLength.Tsid256
        });
        var tsid = factory.Create();
        var end = DateTimeOffset.UtcNow;

        var time = GetTime(tsid);
        var datetime = DateTimeOffset.FromUnixTimeMilliseconds(factory.TsidTimeEpoch.ToUnixTimeMilliseconds() + time);

        var node = GetNode(tsid, 8);
        var sequence = GetSequence(tsid, 14);

        Assert.IsTrue(64 >= Convert.ToString(tsid.Number, 2).Length);
        Assert.AreEqual(13, tsid.ToString().Length);

        Assert.AreEqual(sequence, 0);
        Assert.AreEqual(sourceNode, node);
        Assert.IsTrue(datetime.ToUnixTimeMilliseconds() >= start.ToUnixTimeMilliseconds(), $"{datetime.ToUnixTimeMilliseconds()}<{start.ToUnixTimeMilliseconds()}");
        Assert.IsTrue(datetime.ToUnixTimeMilliseconds() <= end.ToUnixTimeMilliseconds(), $"{datetime.ToUnixTimeMilliseconds()}>{end.ToUnixTimeMilliseconds()}");
    }

    public void Create_Tsid4096()
    {
        const long sourceNode = 20;
        var start = DateTimeOffset.UtcNow;
        var factory = new TsidFactory(new TsidSettings
        {
            Node = sourceNode,
            TsidLength = TsidLength.Tsid256
        });
        var tsid = factory.Create();
        var end = DateTimeOffset.UtcNow;

        var time = GetTime(tsid);
        var datetime = DateTimeOffset.FromUnixTimeMilliseconds(factory.TsidTimeEpoch.ToUnixTimeMilliseconds() + time);

        var node = GetNode(tsid, 12);
        var sequence = GetSequence(tsid, 10);

        Assert.IsTrue(64 >= Convert.ToString(tsid.Number, 2).Length);
        Assert.AreEqual(13, tsid.ToString().Length);

        Assert.AreEqual(sequence, 0);
        Assert.AreEqual(sourceNode, node);
        Assert.IsTrue(datetime.ToUnixTimeMilliseconds() >= start.ToUnixTimeMilliseconds(), $"{datetime.ToUnixTimeMilliseconds()}<{start.ToUnixTimeMilliseconds()}");
        Assert.IsTrue(datetime.ToUnixTimeMilliseconds() <= end.ToUnixTimeMilliseconds(), $"{datetime.ToUnixTimeMilliseconds()}>{end.ToUnixTimeMilliseconds()}");
    }

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

        var time = GetTime(tsid);
        var datetime = DateTimeOffset.FromUnixTimeMilliseconds(factory.TsidTimeEpoch.ToUnixTimeMilliseconds() + time);

        var node = GetNode(tsid, 10);
        var sequence = GetSequence(tsid, 12);

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

    [Test]
    public void From_string()
    {
        const long sourceNode = 21;
        var factory = new TsidFactory(new TsidSettings
        {
            Node = sourceNode
        });
        var tsid = factory.Create();

        var fromTsid = TsidFactory.From(tsid.ToString());

        Assert.AreEqual(tsid.Number, fromTsid.Number);
        Assert.AreNotEqual(tsid, fromTsid);
    }

    [Test]
    public void From_bytes()
    {
        const long sourceNode = 21;
        var factory = new TsidFactory(new TsidSettings
        {
            Node = sourceNode
        });
        var tsid = factory.Create();

        var fromTsid = TsidFactory.From(tsid.ToBytes());

        Assert.AreEqual(tsid.Number, fromTsid.Number);
        Assert.AreNotEqual(tsid, fromTsid);
    }

    [Test]
    public void From_long()
    {
        const long sourceNode = 21;
        var factory = new TsidFactory(new TsidSettings
        {
            Node = sourceNode
        });
        var tsid = factory.Create();

        var fromTsid = TsidFactory.From(tsid.Number);

        Assert.AreEqual(tsid.Number, fromTsid.Number);
        Assert.AreNotEqual(tsid, fromTsid);
    }

    public static long GetTime(Tsid tsid)
    {
        return tsid.Number >> 22;
    }

    private static long GetNode(Tsid tsid, int nodeBitCount)
    {
        return (tsid.Number >> (22 - nodeBitCount)) & (1 << nodeBitCount) - 1;
    }

    private static long GetSequence(Tsid tsid, int sequenceBitCount)
    {
        return tsid.Number & sequenceBitCount;
    }

    private static string[] Sort(string[] secondArr)
    {
        var sorted = new string[secondArr.Length];
        for (var i = 0; i < secondArr.Length; i++)
        {
            sorted[i] = secondArr[i];
        }
        Array.Sort(sorted);
        return sorted;
    }

    private static long[] Sort(long[] secondArr)
    {
        var sorted = new long[secondArr.Length];
        for (var i = 0; i < secondArr.Length; i++)
        {
            sorted[i] = secondArr[i];
        }
        Array.Sort(sorted);
        return sorted;
    }
}
