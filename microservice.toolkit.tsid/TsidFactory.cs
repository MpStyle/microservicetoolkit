using System;

namespace microservice.toolkit.tsid;

public class TsidFactory
{
    private readonly Random seed = new Random();
    private readonly long node;
    private readonly Func<long> nextSequence;

    private long sequence = 0;
    private long lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public TsidFactory() : this(new TsidSettings())
    {
    }

    public TsidFactory(TsidSettings settings)
    {
        var nodeFactory = settings.NodeFactory ?? this.NodeRandom;

        this.node = settings.Node ?? nodeFactory();
        this.nextSequence = settings.SequenceFactory ?? this.NextSequence;

        if (this.IsNodeValid(this.node) == false)
        {
            throw new ArgumentException($"Invalid node value, must be a {TsidProps.NodeBitCount} bits number");
        }
    }

    public Tsid Create()
    {
        lock (this)
        {
            var time = this.Time();
            var timePart = time << TsidProps.NodeBitCount << TsidProps.SequenceBitCount;
            var nodePart = this.node << TsidProps.SequenceBitCount;
            var sequencePart = this.nextSequence();
            return new Tsid(timePart | nodePart | sequencePart);
        }
    }

    private long Time()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (now < this.lastTime || now < TsidProps.TsidTimeEpoch)
        {
            throw new Exception("Invalid system clock");
        }

        this.lastTime = now;

        return now - TsidProps.TsidTimeEpoch;
    }

    private long NextSequence()
    {
        this.sequence %= this.SequenceLimits().Item2;
        return this.sequence++;
    }

    private long NodeRandom()
    {
        var limits = this.NodeLimits();
        return this.LongRandom(limits.Item1, limits.Item2);
    }

    private long LongRandom(long min, long max)
    {
        long result = seed.Next((Int32)(min >> 32), (Int32)(max >> 32));
        result = (result << 32);
        result = result | (long)this.seed.Next((Int32)min, (Int32)max);
        return result;
    }

    private Tuple<long, long> SequenceLimits()
    {
        return this.Limit(TsidProps.SequenceBitCount);
    }

    private bool IsNodeValid(long node)
    {
        var limits = this.NodeLimits();
        return node >= limits.Item1 && node <= limits.Item2;
    }

    private Tuple<long, long> NodeLimits()
    {
        return this.Limit(TsidProps.NodeBitCount);
    }

    private Tuple<long, long> Limit(int bitCount)
    {
        var minValue = 0L;
        var maxValue = (long)Math.Pow(2, bitCount) - 1;

        return Tuple.Create(minValue, maxValue);
    }
}

public class TsidSettings
{
    public long? Node { get; set; }
    public Func<long>? NodeFactory { get; set; }
    public Func<long>? SequenceFactory { get; set; }
}