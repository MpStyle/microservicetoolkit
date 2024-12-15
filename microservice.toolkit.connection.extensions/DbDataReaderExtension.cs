using System;
using System.Data;
using System.Data.Common;

namespace microservice.toolkit.connection.extensions;

public static class DbDataReaderExtension
{
    public static bool TryGetString(this DbDataReader reader, int ordinal, out string value)
    {
        return TryGetValue(reader, ordinal, reader.GetString, out value);
    }
    
    public static string TryGetString(this DbDataReader reader, int ordinal)
    {
        return TryGetValue(reader, ordinal, reader.GetString);
    }

    public static bool TryGetInt64(this DbDataReader reader, int ordinal, out long value)
    {
        return TryGetValue(reader, ordinal, reader.GetInt64, out value);
    }
    
    public static long TryGetInt64(this DbDataReader reader, int ordinal)
    {
        return TryGetValue(reader, ordinal, reader.GetInt64);
    }

    public static bool TryGetInt32(this DbDataReader reader, int ordinal, out int value)
    {
        return TryGetValue(reader, ordinal, reader.GetInt32, out value);
    }

    public static int TryGetInt32(this DbDataReader reader, int ordinal)
    {
        return TryGetValue(reader, ordinal, reader.GetInt32);
    }

    public static bool TryGetInt16(this DbDataReader reader, int ordinal, out short value)
    {
        return TryGetValue(reader, ordinal, reader.GetInt16, out value);
    }

    public static short TryGetInt16(this DbDataReader reader, int ordinal)
    {
        return TryGetValue(reader, ordinal, reader.GetInt16);
    }

    public static bool TryGetBoolean(this DbDataReader reader, int ordinal, out bool value)
    {
        return TryGetValue(reader, ordinal, reader.GetBoolean, out value);
    }

    public static bool TryGetBoolean(this DbDataReader reader, int ordinal)
    {
        return TryGetValue(reader, ordinal, reader.GetBoolean);
    }

    public static bool TryGetByte(this DbDataReader reader, int ordinal, out byte value)
    {
        return TryGetValue(reader, ordinal, reader.GetByte, out value);
    }

    public static byte TryGetByte(this DbDataReader reader, int ordinal)
    {
        return TryGetValue(reader, ordinal, reader.GetByte);
    }

    public static bool TryGetDateTime(this DbDataReader reader, int ordinal, out DateTime value)
    {
        return TryGetValue(reader, ordinal, reader.GetDateTime, out value);
    }

    public static DateTime TryGetDateTime(this DbDataReader reader, int ordinal)
    {
        return TryGetValue(reader, ordinal, reader.GetDateTime);
    }

    public static bool TryGetDecimal(this DbDataReader reader, int ordinal, out decimal value)
    {
        return TryGetValue(reader, ordinal, reader.GetDecimal, out value);
    }

    public static decimal TryGetDecimal(this DbDataReader reader, int ordinal)
    {
        return TryGetValue(reader, ordinal, reader.GetDecimal);
    }

    public static bool TryGetDouble(this DbDataReader reader, int ordinal, out double value)
    {
        return TryGetValue(reader, ordinal, reader.GetDouble, out value);
    }

    public static double TryGetDouble(this DbDataReader reader, int ordinal)
    {
        return TryGetValue(reader, ordinal, reader.GetDouble);
    }

    public static bool TryGetFloat(this DbDataReader reader, int ordinal, out float value)
    {
        return TryGetValue(reader, ordinal, reader.GetFloat, out value);
    }

    public static float TryGetFloat(this DbDataReader reader, int ordinal)
    {
        return TryGetValue(reader, ordinal, reader.GetFloat);
    }

    public static bool TryGetGuid(this DbDataReader reader, int ordinal, out Guid value)
    {
        return TryGetValue(reader, ordinal, reader.GetGuid, out value);
    }

    public static Guid TryGetGuid(this DbDataReader reader, int ordinal)
    {
        return TryGetValue(reader, ordinal, reader.GetGuid);
    }

    public static bool TryGetValue(this DbDataReader reader, int ordinal, out object value)
    {
        return TryGetValue(reader, ordinal, reader.GetValue, out value);
    }

    public static object TryGetValue(this DbDataReader reader, int ordinal)
    {
        return TryGetValue(reader, ordinal, reader.GetValue);
    }

    private static bool TryGetValue<T>(this IDataRecord reader, int ordinal, Func<int, T> func, out T value)
    {
        if (ordinal >=0 && reader.FieldCount > ordinal && reader.IsDBNull(ordinal) == false)
        {
            value = func(ordinal);
            return true;
        }

        value = default;
        return false;
    }
    
    private static T TryGetValue<T>(this IDataRecord reader, int ordinal, Func<int, T> func)
    {
        if (ordinal >=0 && reader.FieldCount > ordinal && reader.IsDBNull(ordinal) == false)
        {
            return func(ordinal);
        }
        
        return default;
    }
}