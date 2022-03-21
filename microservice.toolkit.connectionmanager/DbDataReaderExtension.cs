using System;
using System.Data;
using System.Data.Common;

namespace microservice.toolkit.connectionmanager;

public static class DbDataReaderExtension
{
    public static bool TryGetString(this DbDataReader reader, int ordinal, out string value)
    {
        return TryGetValue(reader, ordinal, reader.GetString, out value);
    }

    public static bool TryGetInt64(this DbDataReader reader, int ordinal, out long value)
    {
        return TryGetValue(reader, ordinal, reader.GetInt64, out value);
    }

    public static bool TryGetInt32(this DbDataReader reader, int ordinal, out int value)
    {
        return TryGetValue(reader, ordinal, reader.GetInt32, out value);
    }

    public static bool TryGetInt16(this DbDataReader reader, int ordinal, out short value)
    {
        return TryGetValue(reader, ordinal, reader.GetInt16, out value);
    }

    public static bool TryGetBoolean(this DbDataReader reader, int ordinal, out bool value)
    {
        return TryGetValue(reader, ordinal, reader.GetBoolean, out value);
    }

    public static bool TryGetByte(this DbDataReader reader, int ordinal, out byte value)
    {
        return TryGetValue(reader, ordinal, reader.GetByte, out value);
    }

    public static bool TryGetDateTime(this DbDataReader reader, int ordinal, out DateTime value)
    {
        return TryGetValue(reader, ordinal, reader.GetDateTime, out value);
    }

    public static bool TryGetDecimal(this DbDataReader reader, int ordinal, out decimal value)
    {
        return TryGetValue(reader, ordinal, reader.GetDecimal, out value);
    }

    public static bool TryGetDouble(this DbDataReader reader, int ordinal, out double value)
    {
        return TryGetValue(reader, ordinal, reader.GetDouble, out value);
    }

    public static bool TryGetFloat(this DbDataReader reader, int ordinal, out float value)
    {
        return TryGetValue(reader, ordinal, reader.GetFloat, out value);
    }

    public static bool TryGetGuid(this DbDataReader reader, int ordinal, out Guid value)
    {
        return TryGetValue(reader, ordinal, reader.GetGuid, out value);
    }

    public static bool TryGetValue(this DbDataReader reader, int ordinal, out object value)
    {
        return TryGetValue(reader, ordinal, reader.GetValue, out value);
    }

    private static bool TryGetValue<T>(this IDataRecord reader, int ordinal, Func<int, T> func, out T value)
    {
        if (reader.FieldCount > ordinal && reader.IsDBNull(ordinal) == false)
        {
            value = func(ordinal);
            return true;
        }

        value = default;
        return false;
    }
}