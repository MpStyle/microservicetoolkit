using System;

namespace microservice.toolkit.tsid;

public static class TsidExtension
{
    private static readonly long[] ALPHABET_VALUES = GetAlphabetValues();

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
        if ((ALPHABET_VALUES[chars[0]] & 0b10000) != 0)
        {
            return false; // overflow!
        }

        for (int i = 0; i < chars.Length; i++)
        {
            if (ALPHABET_VALUES[chars[i]] == -1)
            {
                return false; // invalid character!
            }
        }
        return true; // It seems to be OK.
    }

    private static long[] GetAlphabetValues()
    {
        var alphabetValues = new long[128];
        for (int i = 0; i < alphabetValues.Length; i++)
        {
            alphabetValues[i] = -1;
        }
        // Numbers
        alphabetValues['0'] = 0x00;
        alphabetValues['1'] = 0x01;
        alphabetValues['2'] = 0x02;
        alphabetValues['3'] = 0x03;
        alphabetValues['4'] = 0x04;
        alphabetValues['5'] = 0x05;
        alphabetValues['6'] = 0x06;
        alphabetValues['7'] = 0x07;
        alphabetValues['8'] = 0x08;
        alphabetValues['9'] = 0x09;
        // Lower case
        alphabetValues['a'] = 0x0a;
        alphabetValues['b'] = 0x0b;
        alphabetValues['c'] = 0x0c;
        alphabetValues['d'] = 0x0d;
        alphabetValues['e'] = 0x0e;
        alphabetValues['f'] = 0x0f;
        alphabetValues['g'] = 0x10;
        alphabetValues['h'] = 0x11;
        alphabetValues['j'] = 0x12;
        alphabetValues['k'] = 0x13;
        alphabetValues['m'] = 0x14;
        alphabetValues['n'] = 0x15;
        alphabetValues['p'] = 0x16;
        alphabetValues['q'] = 0x17;
        alphabetValues['r'] = 0x18;
        alphabetValues['s'] = 0x19;
        alphabetValues['t'] = 0x1a;
        alphabetValues['v'] = 0x1b;
        alphabetValues['w'] = 0x1c;
        alphabetValues['x'] = 0x1d;
        alphabetValues['y'] = 0x1e;
        alphabetValues['z'] = 0x1f;
        // Lower case OIL
        alphabetValues['o'] = 0x00;
        alphabetValues['i'] = 0x01;
        alphabetValues['l'] = 0x01;
        // Upper case
        alphabetValues['A'] = 0x0a;
        alphabetValues['B'] = 0x0b;
        alphabetValues['C'] = 0x0c;
        alphabetValues['D'] = 0x0d;
        alphabetValues['E'] = 0x0e;
        alphabetValues['F'] = 0x0f;
        alphabetValues['G'] = 0x10;
        alphabetValues['H'] = 0x11;
        alphabetValues['J'] = 0x12;
        alphabetValues['K'] = 0x13;
        alphabetValues['M'] = 0x14;
        alphabetValues['N'] = 0x15;
        alphabetValues['P'] = 0x16;
        alphabetValues['Q'] = 0x17;
        alphabetValues['R'] = 0x18;
        alphabetValues['S'] = 0x19;
        alphabetValues['T'] = 0x1a;
        alphabetValues['V'] = 0x1b;
        alphabetValues['W'] = 0x1c;
        alphabetValues['X'] = 0x1d;
        alphabetValues['Y'] = 0x1e;
        alphabetValues['Z'] = 0x1f;
        // Upper case OIL
        alphabetValues['O'] = 0x00;
        alphabetValues['I'] = 0x01;
        alphabetValues['L'] = 0x01;

        return alphabetValues;
    }
}
