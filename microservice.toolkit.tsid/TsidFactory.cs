namespace microservice.toolkit.tsid;

public class TsidFactory
{
    private int counter;
    private long lastTime;

    private readonly int node;

    private readonly int nodeBits;
    private readonly int counterBits;

    private readonly int nodeMask;
    private readonly int counterMask;

    private readonly Clock clock;
    private readonly long customEpoch;

    private readonly IRandom random;
    private readonly int randomBytes;

    private static const int NODE_BITS_256 = 8;
    private static const int NODE_BITS_1024 = 10;
    private static const int NODE_BITS_4096 = 12;

    private static const int CLOCK_DRIFT_TOLERANCE = 10_000;

    public TsidFactory()
    {
        this(Builder());
    }

    public TsidFactory(int node)
    {
        this(Builder().withNode(node));
    }

    private TsidFactory(Builder builder)
    {
        this.nodeBits = builder.GetNodeBits();
        this.customEpoch = builder.GetCustomEpoch();
        this.random = builder.GetRandom();
        this.clock = builder.GetClock();

        this.counterBits = RANDOM_BITS - nodeBits;
        this.counterMask = RANDOM_MASK >>> nodeBits;
        this.nodeMask = RANDOM_MASK >>> counterBits;

        this.randomBytes = ((this.counterBits - 1) / 8) + 1;

        this.node = builder.GetNode() & nodeMask;

        this.lastTime = clock.millis();
        this.counter = GetRandomCounter();
    }

    public static TsidFactory NewInstance256()
    {
        return TsidFactory.Builder().withNodeBits(NODE_BITS_256).build();
    }

    public static TsidFactory NewInstance256(int node)
    {
        return TsidFactory.Builder().withNodeBits(NODE_BITS_256).withNode(node).build();
    }

    public static TsidFactory NewInstance1024()
    {
        return TsidFactory.Builder().withNodeBits(NODE_BITS_1024).build();
    }

    public static TsidFactory NewInstance1024(int node)
    {
        return TsidFactory.Builder().withNodeBits(NODE_BITS_1024).withNode(node).build();
    }

    public static TsidFactory NewInstance4096()
    {
        return TsidFactory.Builder().withNodeBits(NODE_BITS_4096).build();
    }

    public static TsidFactory NewInstance4096(int node)
    {
        return TsidFactory.Builder().withNodeBits(NODE_BITS_4096).withNode(node).build();
    }

    public Tsid Create()
    {
        lock (this)
        {
            var _newTime = GetTime() << RANDOM_BITS;
            var newNode = (long)this.node << this.counterBits;
            var newCounter = (long)this.counter & this.counterMask;

            return new Tsid(_newTime | newNode | newCounter);
        }
    }

    private long GetTime()
    {
        lock (this)
        {
            var time = clock.millis();

            if ((time > this.lastTime - CLOCK_DRIFT_TOLERANCE) && (time <= this.lastTime))
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

                switch (bytes.length)
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

    public static Builder Builder()
    {
        return new Builder();
    }

    public static class Builder
    {
        private Integer node;
        private Integer nodeBits;
        private Long customEpoch;
        private IRandom random;
        private Clock clock;

        public Builder WithNode(Integer node)
        {
            this.node = node;
            return this;
        }

        public Builder WithNodeBits(Integer nodeBits)
        {
            if (nodeBits < 0 || nodeBits > 20)
            {
                throw new IllegalArgumentException("Node bits out of range: [0, 20]");
            }

            this.nodeBits = nodeBits;

            return this;
        }

        public Builder WithCustomEpoch(Instant customEpoch)
        {
            this.customEpoch = customEpoch.toEpochMilli();
            return this;
        }

        public Builder WithRandom(Random random)
        {
            if (random != null)
            {
                if (random is SecureRandom)
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

        public Builder WithRandomFunction(IntSupplier randomFunction)
        {
            this.random = new IntRandom(randomFunction);
            return this;
        }

        public Builder WithRandomFunction(IntFunction<byte[]> randomFunction)
        {
            this.random = new ByteRandom(randomFunction);
            return this;
        }

        public Builder WithClock(Clock clock)
        {
            this.clock = clock;
            return this;
        }

        protected Integer GetNode()
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

        protected Integer GetNodeBits()
        {
            if (this.nodeBits == null)
            {
                this.nodeBits = TsidFactory.NODE_BITS_1024; // 10 bits
            }

            return this.nodeBits;
        }

        protected Long GetCustomEpoch()
        {
            if (this.customEpoch == null)
            {
                this.customEpoch = Tsid.TsidEpoch; // 2020-01-01
            }

            return this.customEpoch;
        }

        protected IRandom GetRandom()
        {
            if (this.random == null)
            {
                this.withRandom(new SecureRandom());
            }

            return this.random;
        }

        protected Clock GetClock()
        {
            if (this.clock == null)
            {
                this.withClock(Clock.systemUTC());
            }

            return this.clock;
        }

        public TsidFactory Build()
        {
            return new TsidFactory(this);
        }
    }

    static interface IRandom
    {
        public int NextInt();

        public byte[] NextBytes(int length);
    }

    static class IntRandom : IRandom
    {
        private readonly IntSupplier randomFunction;

        public IntRandom()
        {
            this(NewRandomFunction(null));
        }

        public IntRandom(Random random)
        {
            this(NewRandomFunction(random));
        }

        public IntRandom(IntSupplier randomFunction)
        {
            this.randomFunction = randomFunction != null ? randomFunction : NewRandomFunction(null);
        }

        public int NextInt()
        {
            return randomFunction.getAsInt();
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
                    random = randomFunction.getAsInt();
                }
                shift -= Byte.SIZE; // 56, 48, 40...
                bytes[i] = (byte)(random >>> shift);
            }

            return bytes;
        }

        protected static IntSupplier NewRandomFunction(Random random)
        {
            var entropy = random ??= new SecureRandom();
            return entropy::nextInt;
        }
    }

    static class ByteRandom : IRandom
    {
        private readonly IntFunction<byte[]> randomFunction;

        public ByteRandom()
        {
            this(NewRandomFunction(null));
        }

        public ByteRandom(Random random)
        {
            this(NewRandomFunction(random));
        }

        public ByteRandom(IntFunction<byte[]> randomFunction)
        {
            this.randomFunction = randomFunction != null ? randomFunction : NewRandomFunction(null);
        }

        public int NextInt()
        {
            var number = 0;
            var bytes = this.randomFunction.apply(Integer.BYTES);

            for (var i = 0; i < Integer.BYTES; i++)
            {
                number = (number << 8) | (bytes[i] & 0xff);
            }

            return number;
        }

        public byte[] NextBytes(int length)
        {
            return this.randomFunction.apply(length);
        }

        protected static IntFunction<byte[]> NewRandomFunction(Random random)
        {
            var entropy = random ??= new SecureRandom();

            return (int length) =>
            {
                var bytes = new byte[length];

                entropy.nextBytes(bytes);

                return bytes;
            };
        }
    }
}
