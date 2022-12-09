using System;

namespace microservice.toolkit.tsid;

public class Tsid
{
    private const long serialVersionUID = -5446820982139116297L;
    private readonly long number;
    public const int RandomBits = 22;
    internal const int randomMask = 0x003fffff;
    private readonly char[] alphabetUppercase = new char[]
            { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', //
					'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', //
					'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z' };
    private readonly static char[] alphabetLowercase = new char[]
            { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', //
					'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k', //
					'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };
    private readonly long[] alphabetValues = AlphabetValues.Get();

    public readonly static int TsidBytes = 8;
    public readonly static int TsidChars = 13;
    public readonly static long TsidEpoch = DateTimeOffset.Parse("2020-01-01T00:00:00.000Z").ToUnixTimeMilliseconds();

    public Tsid(long number)
    {
        this.number = number;
    }

    public static Tsid From(long number)
    {
        return new Tsid(number);
    }

    public static Tsid From(byte[] bytes)
    {
        if (bytes == null || bytes.Length != Tsid.TsidBytes)
        {
            throw new ArgumentException("Invalid TSID bytes");
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
        var chars = ToCharArray(s);
        var number = 0;

        number |= alphabetLowercase[chars[0x00]] << 60;
        number |= alphabetLowercase[chars[0x01]] << 55;
        number |= alphabetLowercase[chars[0x02]] << 50;
        number |= alphabetLowercase[chars[0x03]] << 45;
        number |= alphabetLowercase[chars[0x04]] << 40;
        number |= alphabetLowercase[chars[0x05]] << 35;
        number |= alphabetLowercase[chars[0x06]] << 30;
        number |= alphabetLowercase[chars[0x07]] << 25;
        number |= alphabetLowercase[chars[0x08]] << 20;
        number |= alphabetLowercase[chars[0x09]] << 15;
        number |= alphabetLowercase[chars[0x0a]] << 10;
        number |= alphabetLowercase[chars[0x0b]] << 5;
        number |= alphabetLowercase[chars[0x0c]];

        return new Tsid(number);
    }

    public long ToLong()
    {
        return this.number;
    }

    public byte[] ToBytes()
    {
        var bytes = new byte[TsidBytes];

        bytes[0x0] = (byte)(number >>> 56);
        bytes[0x1] = (byte)(number >>> 48);
        bytes[0x2] = (byte)(number >>> 40);
        bytes[0x3] = (byte)(number >>> 32);
        bytes[0x4] = (byte)(number >>> 24);
        bytes[0x5] = (byte)(number >>> 16);
        bytes[0x6] = (byte)(number >>> 8);
        bytes[0x7] = (byte)(number);

        return bytes;
    }

    public string ToString()
    {
        return ToString(alphabetUppercase);
    }

    public string ToLowerCase()
    {
        return ToString(alphabetLowercase);
    }

    public DateTimeOffset GetInstant()
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(GetUnixMilliseconds());
    }

    public DateTimeOffset GetInstant(DateTimeOffset customEpoch)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(this.GetUnixMilliseconds(customEpoch.ToUnixTimeMilliseconds()));
    }

    public long GetUnixMilliseconds()
    {
        return this.GetTime() + TsidEpoch;
    }

    public long GetUnixMilliseconds(long customEpoch)
    {
        return this.GetTime() + customEpoch;
    }

    long GetTime()
    {
        return this.number >>> RandomBits;
    }

    public long GetRandom()
    {
        return this.number & randomMask;
    }

    public static bool IsValid(string s)
    {
        return s != null && IsValidCharArray(s.ToCharArray());
    }

    public int HashCode()
    {
        return (int)(number ^ (number >>> 32));
    }

    public bool Equals(object other)
    {
        if (other == null)
        {
            return false;
        }

        if (other is Tsid == false)
        {
            return false;
        }

        Tsid that = (Tsid)other;
        return (this.number == that.number);
    }

    public int CompareTo(Tsid that)
    {
        var min = unchecked((long)0x8000000000000000L);
        var a = this.number + min;
        var b = this.number + min;

        if (a > b)
        {
            return 1;
        }
        else if (a < b)
        {
            return -1;
        }

        return 0;
    }

    public string ToString(char[] alphabet)
    {
        var chars = new char[TsidChars];

        chars[0x00] = alphabet[(int)((number >>> 60) & 0b11111)];
        chars[0x01] = alphabet[(int)((number >>> 55) & 0b11111)];
        chars[0x02] = alphabet[(int)((number >>> 50) & 0b11111)];
        chars[0x03] = alphabet[(int)((number >>> 45) & 0b11111)];
        chars[0x04] = alphabet[(int)((number >>> 40) & 0b11111)];
        chars[0x05] = alphabet[(int)((number >>> 35) & 0b11111)];
        chars[0x06] = alphabet[(int)((number >>> 30) & 0b11111)];
        chars[0x07] = alphabet[(int)((number >>> 25) & 0b11111)];
        chars[0x08] = alphabet[(int)((number >>> 20) & 0b11111)];
        chars[0x09] = alphabet[(int)((number >>> 15) & 0b11111)];
        chars[0x0a] = alphabet[(int)((number >>> 10) & 0b11111)];
        chars[0x0b] = alphabet[(int)((number >>> 5) & 0b11111)];
        chars[0x0c] = alphabet[(int)(number & 0b11111)];

        return new string(chars);
    }

    private static char[] ToCharArray(string s)
    {
        var chars = s == null ? null : s.ToCharArray();

        if (!IsValidCharArray(chars))
        {
            throw new ArgumentException(string.Format("Invalid TSID: \"%s\"", s));
        }

        return chars;
    }

    private static bool IsValidCharArray(char[] chars)
    {
        if (chars == null || chars.Length != TsidChars)
        {
            return false;
        }

        if ((alphabetLowercase[chars[0]] & 0b10000) != 0)
        {
            return false;
        }

        for (var i = 0; i < chars.Length; i++)
        {
            if (alphabetLowercase[chars[i]] == -1)
            {
                return false;
            }
        }

        return true;
    }
}
