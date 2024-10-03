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

        Assert.That(tsids.Length, Is.EqualTo(sortedTsids.Length));
        for (var i = 0; i < tsids.Length; i++)
        {
            Assert.That(tsids[i], Is.EqualTo(sortedTsids[i]));
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

        Assert.That(tsids.Length, Is.EqualTo(sortedTsids.Length));
        for (var i = 0; i < tsids.Length; i++)
        {
            Assert.That(tsids[i], Is.EqualTo(sortedTsids[i]));
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

        Assert.That(tsids.Length, Is.EqualTo(sortedTsids.Length));
        for (var i = 0; i < tsids.Length; i++)
        {
            Assert.That(tsids[i], Is.EqualTo(sortedTsids[i]));
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

        Assert.That(tsids.Length, Is.EqualTo(sortedTsids.Length));
        for (var i = 0; i < tsids.Length; i++)
        {
            Assert.That(tsids[i], Is.EqualTo(sortedTsids[i]));
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

        Assert.That(tsids.Length, Is.EqualTo(sortedTsids.Length));
        for (var i = 0; i < tsids.Length; i++)
        {
            Assert.That(tsids[i], Is.EqualTo(sortedTsids[i]));
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

        Assert.That(tsids.Length, Is.EqualTo(sortedTsids.Length));
        for (var i = 0; i < tsids.Length; i++)
        {
            Assert.That(tsids[i], Is.EqualTo(sortedTsids[i]));
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

        Assert.That(64 >= Convert.ToString(tsid.Number, 2).Length, Is.True);
        Assert.That(13, Is.EqualTo(tsid.ToString().Length));

        Assert.That(sequence, Is.EqualTo(0));
        Assert.That(sourceNode, Is.EqualTo(node));
        Assert.That(datetime.ToUnixTimeMilliseconds() >= start.ToUnixTimeMilliseconds(), Is.True, $"{datetime.ToUnixTimeMilliseconds()}<{start.ToUnixTimeMilliseconds()}");
        Assert.That(datetime.ToUnixTimeMilliseconds() <= end.ToUnixTimeMilliseconds(), Is.True, $"{datetime.ToUnixTimeMilliseconds()}>{end.ToUnixTimeMilliseconds()}");
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

        Assert.That(64 >= Convert.ToString(tsid.Number, 2).Length, Is.True);
        Assert.That(13, Is.EqualTo(tsid.ToString().Length));

        Assert.That(sequence, Is.EqualTo(0));
        Assert.That(sourceNode, Is.EqualTo(node));
        Assert.That(datetime.ToUnixTimeMilliseconds() >= start.ToUnixTimeMilliseconds(), Is.True, $"{datetime.ToUnixTimeMilliseconds()}<{start.ToUnixTimeMilliseconds()}");
        Assert.That(datetime.ToUnixTimeMilliseconds() <= end.ToUnixTimeMilliseconds(), Is.True, $"{datetime.ToUnixTimeMilliseconds()}>{end.ToUnixTimeMilliseconds()}");
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

        Assert.That(64 >= Convert.ToString(tsid.Number, 2).Length, Is.True);
        Assert.That(13, Is.EqualTo(tsid.ToString().Length));

        Assert.That(sequence, Is.EqualTo(0));
        Assert.That(sourceNode, Is.EqualTo(node));
        Assert.That(datetime.ToUnixTimeMilliseconds() >= start.ToUnixTimeMilliseconds(), Is.True, $"{datetime.ToUnixTimeMilliseconds()}<{start.ToUnixTimeMilliseconds()}");
        Assert.That(datetime.ToUnixTimeMilliseconds() <= end.ToUnixTimeMilliseconds(), Is.True, $"{datetime.ToUnixTimeMilliseconds()}>{end.ToUnixTimeMilliseconds()}");
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

        Assert.That(64 >= Convert.ToString(tsid.Number, 2).Length, Is.True);
        Assert.That(13, Is.EqualTo(tsid.ToString().Length));

        Assert.That(sequence, Is.EqualTo(0));
        Assert.That(sourceNode, Is.EqualTo(node));
        Assert.That(datetime.ToUnixTimeMilliseconds() >= start.ToUnixTimeMilliseconds(), Is.True, $"{datetime.ToUnixTimeMilliseconds()}<{start.ToUnixTimeMilliseconds()}");
        Assert.That(datetime.ToUnixTimeMilliseconds() <= end.ToUnixTimeMilliseconds(), Is.True, $"{datetime.ToUnixTimeMilliseconds()}>{end.ToUnixTimeMilliseconds()}");
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

        Assert.That(64 >= Convert.ToString(first.Number, 2).Length, Is.True);
        Assert.That(13, Is.EqualTo(first.ToString().Length));

        Assert.That(64 >= Convert.ToString(second.Number, 2).Length, Is.True);
        Assert.That(13, Is.EqualTo(second.ToString().Length));

        Assert.That(64 >= Convert.ToString(third.Number, 2).Length, Is.True);
        Assert.That(13, Is.EqualTo(third.ToString().Length));

        Assert.That(64 >= Convert.ToString(fourth.Number, 2).Length, Is.True);
        Assert.That(13, Is.EqualTo(fourth.ToString().Length));

        Assert.That(64 >= Convert.ToString(fifth.Number, 2).Length, Is.True);
        Assert.That(13, Is.EqualTo(fifth.ToString().Length));

        Assert.That(first.Number < second.Number, Is.True);
        Assert.That(second.Number < third.Number, Is.True);
        Assert.That(third.Number < fourth.Number, Is.True);
        Assert.That(fourth.Number < fifth.Number, Is.True);
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

        Assert.That(64 >= Convert.ToString(tsid.Number, 2).Length, Is.True);
        Assert.That(13, Is.EqualTo(tsid.ToString().Length));

        Assert.That(tsid.Number, Is.Not.Null);
        Assert.That(tsid.ToString(), Is.Not.Null);
        Assert.That(tsid.ToLowerCase(), Is.Not.Null);
        Assert.That(string.Equals(tsid.ToString(), tsid.ToLowerCase(), StringComparison.OrdinalIgnoreCase), Is.True);
        Assert.That(tsid.ToBytes(), Is.Not.Null);
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

        Assert.That(tsid.Number, Is.EqualTo(fromTsid.Number));
        Assert.That(tsid, Is.Not.EqualTo( fromTsid));
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

        Assert.That(tsid.Number, Is.EqualTo(fromTsid.Number));
        Assert.That(tsid, Is.Not.EqualTo( fromTsid));
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

        Assert.That(tsid.Number, Is.EqualTo(fromTsid.Number));
        Assert.That(tsid, Is.Not.EqualTo( fromTsid));
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
