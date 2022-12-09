using System;

namespace microservice.toolkit.tsid;

internal class Integer
{
    internal static readonly int BYTES = 4;
    internal static readonly int SIZE = 32;
}

internal class Byte
{
    internal static readonly int SIZE = 8;
}

public class TsidFactory
{
    private int counter;
    private long lastTime;

    private readonly int? node;

    private readonly int? nodeBits;
    private readonly int counterBits;

    private readonly int nodeMask;
    private readonly int counterMask;

    private readonly DateTimeOffset clock;
    private readonly long? customEpoch;

    private readonly IRandom random;
    private readonly int randomBytes;

    public const int NodeBits256 = 8;
    public const int NodeBits1024 = 10;
    public const int NodeBits4096 = 12;

    private const int ClockDriftTolerance = 10_000;

    public TsidFactory() : this(new Builder())
    {
    }

    public TsidFactory(int node) : this(new Builder().WithNode(node))
    {
    }

    private TsidFactory(Builder builder)
    {
        this.nodeBits = builder.GetNodeBits();
        this.customEpoch = builder.GetCustomEpoch();
        this.random = builder.GetRandom();
        this.clock = builder.GetClock();

        this.counterBits = (int)(Tsid.RandomBits - nodeBits);
        this.counterMask = (int)(Tsid.randomMask >>> nodeBits);
        this.nodeMask = Tsid.randomMask >>> counterBits;

        this.randomBytes = ((this.counterBits - 1) / 8) + 1;

        this.node = builder.GetNode() & nodeMask;

        this.lastTime = clock.ToUnixTimeMilliseconds();
        this.counter = GetRandomCounter();
    }

    public static TsidFactory NewInstance256()
    {
        return new Builder().WithNodeBits(NodeBits256).Build();
    }

    public static TsidFactory NewInstance256(int node)
    {
        return new Builder().WithNodeBits(NodeBits256).WithNode(node).Build();
    }

    public static TsidFactory NewInstance1024()
    {
        return new Builder().WithNodeBits(NodeBits1024).Build();
    }

    public static TsidFactory NewInstance1024(int node)
    {
        return new Builder().WithNodeBits(NodeBits1024).WithNode(node).Build();
    }

    public static TsidFactory NewInstance4096()
    {
        return new Builder().WithNodeBits(NodeBits4096).Build();
    }

    public static TsidFactory NewInstance4096(int node)
    {
        return new Builder().WithNodeBits(NodeBits4096).WithNode(node).Build();
    }

    public Tsid Create()
    {
        lock (this)
        {
            var newTime = (long)(GetTime() << Tsid.RandomBits);
            var newNode = (long)this.node << this.counterBits;
            var newCounter = (long)this.counter & this.counterMask;

            return new Tsid(newTime | newNode | newCounter);
        }
    }

    private long? GetTime()
    {
        lock (this)
        {
            var time = clock.ToUnixTimeMilliseconds();

            if ((time > this.lastTime - ClockDriftTolerance) && (time <= this.lastTime))
            {
                this.counter++;

                var carry = this.counter >>> this.counterBits;

                this.counter = this.counter & this.counterMask;

                time = this.lastTime + carry; // increment time
            }
            else
            {
                this.counter = this.GetRandomCounter();
            }

            this.lastTime = time;

            return time - this.customEpoch;
        }
    }

    private int GetRandomCounter()
    {
        lock (this)
        {
            if (random is ByteRandom)
            {
                var bytes = random.NextBytes(this.randomBytes);

                switch (bytes.Length)
                {
                    case 1:
                        return (bytes[0] & 0xff) & this.counterMask;
                    case 2:
                        return (((bytes[0] & 0xff) << 8) | (bytes[1] & 0xff)) & this.counterMask;
                    default:
                        return (((bytes[0] & 0xff) << 16) | ((bytes[1] & 0xff) << 8) | (bytes[2] & 0xff)) & this.counterMask;
                }
            }
            else
            {
                return random.NextInt() & this.counterMask;
            }
        }
    }

    public class Builder
    {
        private int? node;
        private int? nodeBits;
        private long? customEpoch;
        private IRandom random;
        private DateTimeOffset? clock;

        public Builder WithNode(int? node)
        {
            this.node = node;
            return this;
        }

        public Builder WithNodeBits(int? nodeBits)
        {
            if (nodeBits < 0 || nodeBits > 20)
            {
                throw new ArgumentException("Node bits out of range: [0, 20]");
            }

            this.nodeBits = nodeBits;

            return this;
        }

        public Builder WithCustomEpoch(DateTimeOffset customEpoch)
        {
            this.customEpoch = customEpoch.ToUnixTimeMilliseconds();
            return this;
        }

        public Builder WithRandom(Random random)
        {
            if (random != null)
            {
                if (random is Random)
                {
                    this.random = new ByteRandom(random);
                }
                else
                {
                    this.random = new IntRandom(random);
                }
            }
            return this;
        }

        public Builder WithRandomFunction(Func<int> randomFunction)
        {
            this.random = new IntRandom(randomFunction);
            return this;
        }

        public Builder WithRandomFunction(Func<int, byte[]> randomFunction)
        {
            this.random = new ByteRandom(randomFunction);
            return this;
        }

        public Builder WithClock(DateTimeOffset clock)
        {
            this.clock = clock;
            return this;
        }

        internal int? GetNode()
        {
            int mask = 0x3fffff;

            if (this.node == null)
            {
                if (SettingsUtil.getNode() != null)
                {
                    this.node = SettingsUtil.getNode();
                }
                else
                {
                    this.node = this.random.NextInt();
                }
            }

            return this.node & mask;
        }

        internal int? GetNodeBits()
        {
            if (this.nodeBits == null)
            {
                this.nodeBits = TsidFactory.NodeBits1024; // 10 bits
            }

            return this.nodeBits;
        }

        internal long? GetCustomEpoch()
        {
            if (this.customEpoch == null)
            {
                this.customEpoch = Tsid.TsidEpoch; // 2020-01-01
            }

            return this.customEpoch;
        }

        internal IRandom GetRandom()
        {
            if (this.random == null)
            {
                this.WithRandom(new Random());
            }

            return this.random;
        }

        internal DateTimeOffset GetClock()
        {
            if (this.clock == null)
            {
                this.WithClock(DateTimeOffset.UtcNow);
            }

            return this.clock.Value;
        }

        public TsidFactory Build()
        {
            return new TsidFactory(this);
        }
    }

    internal interface IRandom
    {
        public int NextInt();

        public byte[] NextBytes(int length);
    }

    internal class IntRandom : IRandom
    {
        private readonly Func<int> randomFunction;

        public IntRandom() : this(NewRandomFunction(null))
        {
        }

        public IntRandom(Random random) : this(NewRandomFunction(random))
        {
        }

        public IntRandom(Func<int> randomFunction)
        {
            this.randomFunction = randomFunction != null ? randomFunction : NewRandomFunction(null);
        }

        public int NextInt()
        {
            return randomFunction();
        }

        public byte[] NextBytes(int length)
        {
            var shift = 0;
            var random = 0;
            var bytes = new byte[length];

            for (var i = 0; i < length; i++)
            {
                if (shift < Byte.SIZE)
                {
                    shift = Integer.SIZE;
                    random = randomFunction();
                }
                shift -= Byte.SIZE; // 56, 48, 40...
                bytes[i] = (byte)(random >>> shift);
            }

            return bytes;
        }

        protected static Func<int> NewRandomFunction(Random random)
        {
            return () => (random ??= new Random()).Next();
        }
    }

    internal class ByteRandom : IRandom
    {
        private readonly Func<int, byte[]> randomFunction;

        public ByteRandom() : this(NewRandomFunction(null))
        {
        }

        public ByteRandom(Random random) : this(NewRandomFunction(random))
        {
        }

        public ByteRandom(Func<int, byte[]> randomFunction)
        {
            this.randomFunction = randomFunction != null ? randomFunction : NewRandomFunction(null);
        }

        public int NextInt()
        {
            var number = 0;
            var bytes = this.randomFunction(Integer.BYTES);

            for (var i = 0; i < Integer.BYTES; i++)
            {
                number = (number << 8) | (bytes[i] & 0xff);
            }

            return number;
        }

        public byte[] NextBytes(int length)
        {
            return this.randomFunction(length);
        }

        protected static Func<int, byte[]> NewRandomFunction(Random random)
        {
            var entropy = random ??= new Random();

            return (int length) =>
            {
                var bytes = new byte[length];

                entropy.NextBytes(bytes);

                return bytes;
            };
        }
    }
}
