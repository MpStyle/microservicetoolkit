using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.connection.extensions;

public static class DbDataReaderExtension
{
    public static bool TryGetString(this DbDataReader reader, int ordinal, out string value)
    {
        return TryGetValue(reader, ordinal, reader.GetString, out value);
    }

    public static string? GetNullableString(this DbDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }
    
    public static async Task<string?> GetNullableStringAsync(this DbDataReader reader, int ordinal, CancellationToken cancellationToken = default)
    {
        return await reader.IsDBNullAsync(ordinal, cancellationToken) ? null : reader.GetString(ordinal);
    }

    public static bool TryGetInt64(this DbDataReader reader, int ordinal, out long value)
    {
        return TryGetValue(reader, ordinal, reader.GetInt64, out value);
    }
    
    public static long? GetNullableInt64(this DbDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
    }
    
    public static async Task<long?> GetNullableInt64Async(this DbDataReader reader, int ordinal, CancellationToken cancellationToken = default)
    {
        return await reader.IsDBNullAsync(ordinal, cancellationToken) ? null : reader.GetInt64(ordinal);
    }

    public static bool TryGetInt32(this DbDataReader reader, int ordinal, out int value)
    {
        return TryGetValue(reader, ordinal, reader.GetInt32, out value);
    }
    
    public static int? GetNullableInt32(this DbDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }
    
    public static async Task<int?> GetNullableInt32Async(this DbDataReader reader, int ordinal, CancellationToken cancellationToken = default)
    {
        return await reader.IsDBNullAsync(ordinal, cancellationToken) ? null : reader.GetInt32(ordinal);
    }

    public static bool TryGetInt16(this DbDataReader reader, int ordinal, out short value)
    {
        return TryGetValue(reader, ordinal, reader.GetInt16, out value);
    }
    
    public static short? GetNullableInt16(this DbDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetInt16(ordinal);
    }
    
    public static async Task<short?> GetNullableInt16Async(this DbDataReader reader, int ordinal, CancellationToken cancellationToken = default)
    {
        return await reader.IsDBNullAsync(ordinal, cancellationToken) ? null : reader.GetInt16(ordinal);
    }

    public static bool TryGetBoolean(this DbDataReader reader, int ordinal, out bool value)
    {
        return TryGetValue(reader, ordinal, reader.GetBoolean, out value);
    }
    
    public static bool? GetNullableBoolean(this DbDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetBoolean(ordinal);
    }
    
    public static async Task<bool?> GetNullableBooleanAsync(this DbDataReader reader, int ordinal, CancellationToken cancellationToken = default)
    {
        return await reader.IsDBNullAsync(ordinal, cancellationToken) ? null : reader.GetBoolean(ordinal);
    }

    public static bool TryGetByte(this DbDataReader reader, int ordinal, out byte value)
    {
        return TryGetValue(reader, ordinal, reader.GetByte, out value);
    }
    
    public static byte? GetNullableByte(this DbDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetByte(ordinal);
    }
    
    public static async Task<byte?> GetNullableByteAsync(this DbDataReader reader, int ordinal, CancellationToken cancellationToken = default)
    {
        return await reader.IsDBNullAsync(ordinal, cancellationToken) ? null : reader.GetByte(ordinal);
    }

    public static bool TryGetDateTime(this DbDataReader reader, int ordinal, out DateTime value)
    {
        return TryGetValue(reader, ordinal, reader.GetDateTime, out value);
    }
    
    public static DateTime? GetNullableDateTime(this DbDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }
    
    public static async Task<DateTime?> GetNullableDateTimeAsync(this DbDataReader reader, int ordinal, CancellationToken cancellationToken = default)
    {
        return await reader.IsDBNullAsync(ordinal, cancellationToken) ? null : reader.GetDateTime(ordinal);
    }

    public static bool TryGetDecimal(this DbDataReader reader, int ordinal, out decimal value)
    {
        return TryGetValue(reader, ordinal, reader.GetDecimal, out value);
    }
    
    public static decimal? GetNullableDecimal(this DbDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
    }
    
    public static async Task<decimal?> GetNullableDecimalAsync(this DbDataReader reader, int ordinal, CancellationToken cancellationToken = default)
    {
        return await reader.IsDBNullAsync(ordinal, cancellationToken) ? null : reader.GetDecimal(ordinal);
    }

    public static bool TryGetDouble(this DbDataReader reader, int ordinal, out double value)
    {
        return TryGetValue(reader, ordinal, reader.GetDouble, out value);
    }
    
    public static double? GetNullableDouble(this DbDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetDouble(ordinal);
    }
    
    public static async Task<double?> GetNullableDoubleAsync(this DbDataReader reader, int ordinal, CancellationToken cancellationToken = default)
    {
        return await reader.IsDBNullAsync(ordinal, cancellationToken) ? null : reader.GetDouble(ordinal);
    }

    public static bool TryGetFloat(this DbDataReader reader, int ordinal, out float value)
    {
        return TryGetValue(reader, ordinal, reader.GetFloat, out value);
    }
    
    public static float? GetNullableFloat(this DbDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetFloat(ordinal);
    }
    
    public static async Task<float?> GetNullableFloatAsync(this DbDataReader reader, int ordinal, CancellationToken cancellationToken = default)
    {
        return await reader.IsDBNullAsync(ordinal, cancellationToken) ? null : reader.GetFloat(ordinal);
    }

    public static bool TryGetGuid(this DbDataReader reader, int ordinal, out Guid value)
    {
        return TryGetValue(reader, ordinal, reader.GetGuid, out value);
    }
    
    public static Guid? GetNullableGuid(this DbDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetGuid(ordinal);
    }
    
    public static async Task<Guid?> GetNullableGuidAsync(this DbDataReader reader, int ordinal, CancellationToken cancellationToken = default)
    {
        return await reader.IsDBNullAsync(ordinal, cancellationToken) ? null : reader.GetGuid(ordinal);
    }

    public static bool TryGetValue(this DbDataReader reader, int ordinal, out object value)
    {
        return TryGetValue(reader, ordinal, reader.GetValue, out value);
    }
    
    public static object? GetNullableValue(this DbDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);
    }
    
    public static async Task<object?> GetNullableValueAsync(this DbDataReader reader, int ordinal, CancellationToken cancellationToken = default)
    {
        return await reader.IsDBNullAsync(ordinal, cancellationToken) ? null : reader.GetValue(ordinal);
    }

    private static bool TryGetValue<T>(this IDataRecord reader, int ordinal, Func<int, T> func, out T value)
    {
        if (IsNullable<T>() && reader.IsDBNull(ordinal))
        {
            value = default;
            return true;
        }

        if (ordinal >= 0 && reader.FieldCount > ordinal && reader.IsDBNull(ordinal) == false)
        {
            value = func(ordinal);
            return true;
        }

        value = default;
        return false;
    }

    private static bool IsNullable<T>()
    {
        var type = typeof(T);
        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }
}