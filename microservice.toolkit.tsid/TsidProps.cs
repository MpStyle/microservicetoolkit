using System;
using System.Globalization;

namespace microservice.toolkit.tsid;

public class TsidProps
{
    public static readonly long TsidTimeEpoch = DateTimeOffset.ParseExact("10/12/2022 08.15.00 +01:00", "dd/MM/yyyy HH.mm.ss zzz", CultureInfo.InvariantCulture).ToUnixTimeMilliseconds();

    public const int ByteCount = 8;
    public const int CharCount = 13;

    public const int TimeBitCount = 41;
    public const int NodeBitCount = 10;
    public const int SequenceBitCount = 12;

    internal static readonly char[] ALPHABET_UPPERCASE = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                    'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K',
                    'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z' };

    internal static readonly char[] ALPHABET_LOWERCASE = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                    'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k',
                    'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };
}
