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

    public static Tsid From(long number)
    {
        return new Tsid(number);
    }

    public static Tsid From(byte[] bytes)
    {

        if (bytes == null || bytes.Length != TsidProps.ByteCount)
        {
            throw new ArgumentException("Invalid TSID bytes"); // null or wrong length!
        }

        long number = 0;

        number |= (bytes[0x0] & 0xffL) << 56;
        number |= (bytes[0x1] & 0xffL) << 48;
        number |= (bytes[0x2] & 0xffL) << 40;
        number |= (bytes[0x3] & 0xffL) << 32;
        number |= (bytes[0x4] & 0xffL) << 24;
        number |= (bytes[0x5] & 0xffL) << 16;
        number |= (bytes[0x6] & 0xffL) << 8;
        number |= (bytes[0x7] & 0xffL);

        return new Tsid(number);
    }

    public static Tsid From(string s)
    {
        var chars = TsidFactory.ToCharArray(s);
        long number = 0;

        number |= TsidProps.ALPHABET_VALUES[chars[0x00]] << 60;
        number |= TsidProps.ALPHABET_VALUES[chars[0x01]] << 55;
        number |= TsidProps.ALPHABET_VALUES[chars[0x02]] << 50;
        number |= TsidProps.ALPHABET_VALUES[chars[0x03]] << 45;
        number |= TsidProps.ALPHABET_VALUES[chars[0x04]] << 40;
        number |= TsidProps.ALPHABET_VALUES[chars[0x05]] << 35;
        number |= TsidProps.ALPHABET_VALUES[chars[0x06]] << 30;
        number |= TsidProps.ALPHABET_VALUES[chars[0x07]] << 25;
        number |= TsidProps.ALPHABET_VALUES[chars[0x08]] << 20;
        number |= TsidProps.ALPHABET_VALUES[chars[0x09]] << 15;
        number |= TsidProps.ALPHABET_VALUES[chars[0x0a]] << 10;
        number |= TsidProps.ALPHABET_VALUES[chars[0x0b]] << 5;
        number |= TsidProps.ALPHABET_VALUES[chars[0x0c]];

        return new Tsid(number);
    }

    private static char[] ToCharArray(string s)
    {
        var chars = s == null ? null : s.ToCharArray();
        if (!TsidFactory.IsValidCharArray(chars))
        {
            throw new ArgumentException(string.Format("Invalid TSID: \"%s\"", s));
        }
        return chars;
    }

    private static bool IsValidCharArray(char[] chars)
    {

        if (chars == null || chars.Length != TsidProps.CharCount)
        {
            return false; // null or wrong size!
        }

        // The extra bit added by base-32 encoding must be zero
        // As a consequence, the 1st char of the input string must be between 0 and F.
        if ((TsidProps.ALPHABET_VALUES[chars[0]] & 0b10000) != 0)
        {
            return false; // overflow!
        }

        for (var i = 0; i < chars.Length; i++)
        {
            if (TsidProps.ALPHABET_VALUES[chars[i]] == -1)
            {
                return false; // invalid character!
            }
        }
        return true; // It seems to be OK.
    }
}

public class TsidSettings
{
    public int? NodeBitCount { get; set; }
    public long? Node { get; set; }
    public Func<long>? NodeFactory { get; set; }
    public Func<long>? SequenceFactory { get; set; }
}