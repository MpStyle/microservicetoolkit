using System;
using System.Globalization;

namespace microservice.toolkit.tsid;

public class TsidProps
{
    public static readonly long TsidTimeEpoch = DateTimeOffset.ParseExact("10/12/2022 08.15.00 +01:00", "dd/MM/yyyy HH.mm.ss zzz", CultureInfo.InvariantCulture).ToUnixTimeMilliseconds();

    internal const int ByteCount = 8;
    internal const int CharCount = 13;

    internal static readonly char[] ALPHABET_UPPERCASE = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                    'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K',
                    'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z' };

    internal static readonly char[] ALPHABET_LOWERCASE = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                    'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k',
                    'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };

    internal static readonly long[] ALPHABET_VALUES = GetAlphabetValues();

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
