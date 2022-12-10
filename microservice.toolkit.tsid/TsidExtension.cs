using System;

namespace microservice.toolkit.tsid;

public static class TsidExtension
{

    public static long GetTime(this Tsid tsid)
    {
        return tsid.Number >> TsidProps.NodeBitCount >> TsidProps.SequenceBitCount;
    }

    public static DateTimeOffset GetDateTimeOffset(this Tsid tsid)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(tsid.GetTime());
    }

    public static long GetNode(this Tsid tsid)
    {
        return (tsid.Number >> TsidProps.SequenceBitCount) & (1 << TsidProps.NodeBitCount) - 1;
    }

    public static long GetSequence(this Tsid tsid)
    {
        return tsid.Number & TsidProps.SequenceBitCount;
    }

    public static byte[] ToBytes(this Tsid tsid)
    {
        var number = tsid.Number;
        byte[] bytes = new byte[TsidProps.ByteCount];

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
    public static string ToLowerCase(this Tsid tsid)
    {
        return tsid.ToString(TsidProps.ALPHABET_LOWERCASE);
    }

    internal static string ToString(this Tsid tsid, char[] alphabet)
    {
        var number = tsid.Number;
        var chars = new char[TsidProps.CharCount];

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

    internal static char[] ToCharArray(string s)
    {
        var chars = s == null ? null : s.ToCharArray();
        if (!IsValidCharArray(chars))
        {
            throw new ArgumentException(String.Format("Invalid TSID: \"%s\"", s));
        }
        return chars;
    }

    internal static bool IsValidCharArray(char[] chars)
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

        for (int i = 0; i < chars.Length; i++)
        {
            if (TsidProps.ALPHABET_VALUES[chars[i]] == -1)
            {
                return false; // invalid character!
            }
        }
        return true; // It seems to be OK.
    }
}
