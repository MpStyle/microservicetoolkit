using microservice.toolkit.connection.extensions.objectmapper;
using microservice.toolkit.core.extension;

using Microsoft.Data.SqlClient;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.connection.extensions;

public static class SQLConnectionExtension
{
    public static SqlParameter ToDbParameter(this SqlCommand command, string name, object? value)
    {
        var param = command.CreateParameter();

        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;

        var dbType = DbType.Int64;
        if (value != null)
        {
            dbType = value.GetType().IsEnum ? DbMapper.TypeMapper[typeof(int)] : DbMapper.TypeMapper[value.GetType()];
        }

        param.DbType = dbType;

        return param;
    }

    public static int ExecuteStoredProcedure(this SqlConnection conn, string storedProcedureName,
        Dictionary<string, object>? parameters = null)
    {
        conn.SafeOpen();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = storedProcedureName;

        cmd.CommandType = CommandType.StoredProcedure;

        if (parameters.IsNullOrEmpty() == false)
        {
            cmd.Parameters.AddRange(parameters.ToDbParameter(cmd));
        }

        return cmd.ExecuteNonQuery();
    }

    public static T Execute<T>(this SqlConnection conn, Func<SqlCommand, T> lambda)
    {
        conn.SafeOpen();
        using var cmd = conn.CreateCommand();
        return lambda(cmd);
    }

    public static T[] Execute<T>(this SqlConnection conn, string sql, Func<SqlDataReader, T> lambda,
        Dictionary<string, object>? parameters = null)
    {
        return conn.Execute(command =>
        {
            command.CommandText = sql;

            if (parameters.IsNullOrEmpty() == false)
            {
                command.Parameters.AddRange(parameters.ToDbParameter(command));
            }

            using var reader = command.ExecuteReader();
            var objects = new List<T>();

            while (reader.Read())
            {
                objects.Add(lambda(reader));
            }

            return objects.ToArray();
        });
    }

    public static T? ExecuteFirst<T>(this SqlConnection conn, string sql, Func<SqlDataReader, T> lambda,
        Dictionary<string, object>? parameters = null)
    {
        var result = conn.Execute(sql, lambda, parameters);

        return result.IsNullOrEmpty() ? default : result.First();
    }

    public static async Task<int> ExecuteStoredProcedureAsync(this SqlConnection conn, string storedProcedureName,
        Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        await conn.SafeOpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = storedProcedureName;

        cmd.CommandType = CommandType.StoredProcedure;

        if (parameters.IsNullOrEmpty() == false)
        {
            var values = parameters.ToDbParameter(cmd);
            cmd.Parameters.AddRange(values);
        }

        return await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public static async Task<T> ExecuteAsync<T>(this SqlConnection conn, Func<SqlCommand, Task<T>> lambda, CancellationToken cancellationToken = default)
    {
        await conn.SafeOpenAsync(cancellationToken: cancellationToken);
        await using var cmd = conn.CreateCommand();
        return await lambda(cmd);
    }

    public static async Task<T[]> ExecuteAsync<T>(this SqlConnection conn, string sql,
        Func<SqlDataReader, T> lambda,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return await conn.ExecuteAsync(async command =>
        {
            command.CommandText = sql;

            if (parameters.IsNullOrEmpty() == false)
            {
                command.Parameters.AddRange(parameters.ToDbParameter(command));
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var objects = new List<T>();

            while (await reader.ReadAsync(cancellationToken))
            {
                objects.Add(lambda(reader));
            }

            return objects.ToArray();
        }, cancellationToken: cancellationToken);
    }

    public static async Task<T[]> ExecuteAsync<T>(this SqlConnection conn, string sql,
        Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default) where T : class, new()
    {
        return await conn.ExecuteAsync(sql, MapperFunc<T>(), parameters, cancellationToken: cancellationToken);
    }

    public static async Task<T?> ExecuteFirstAsync<T>(this SqlConnection conn, string sql,
        Func<SqlDataReader, T> lambda,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var result = await conn.ExecuteAsync(sql, lambda, parameters, cancellationToken: cancellationToken);

        return result.IsNullOrEmpty() ? default : result.First();
    }

    public static async Task<T?> ExecuteFirstAsync<T>(this SqlConnection conn, string sql,
        Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default) where T : class, new()
    {
        return await conn.ExecuteFirstAsync(sql, MapperFunc<T>(), parameters, cancellationToken: cancellationToken);
    }

    public static int ExecuteNonQuery(this SqlConnection conn, string query,
        Dictionary<string, object>? parameters = null)
    {
        conn.SafeOpen();
        using var command = conn.CreateCommand();
        command.CommandText = query;

        if (parameters.IsNullOrEmpty() == false)
        {
            command.Parameters.AddRange(parameters.ToDbParameter(command));
        }

        return command.ExecuteNonQuery();
    }

    public static async Task<int> ExecuteNonQueryAsync(this SqlConnection conn, string query,
        Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        await conn.SafeOpenAsync(cancellationToken: cancellationToken);
        await using var command = conn.CreateCommand();
        command.CommandText = query;

        if (parameters.IsNullOrEmpty() == false)
        {
            command.Parameters.AddRange(parameters.ToDbParameter(command));
        }

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// It opens a database connection.
    /// </summary>
    public static void SafeOpen(this SqlConnection conn)
    {
        if (conn.State.HasFlag(ConnectionState.Open))
        {
            return;
        }

        if (conn.State.HasFlag(ConnectionState.Closed))
        {
            conn.Open();
            return;
        }

        if (conn.State.HasFlag(ConnectionState.Broken))
        {
            conn.Close();
            conn.Open();
            return;
        }
    }

    /// <summary>
    /// An asynchronous version of microservice.toolkit.connection.extensions.DbConnectionExtensions.Open, which opens
    /// a database connection.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SafeOpenAsync(this SqlConnection conn, CancellationToken cancellationToken = default)
    {
        if (conn.State.HasFlag(ConnectionState.Open))
        {
            return;
        }

        if (conn.State.HasFlag(ConnectionState.Closed))
        {
            await conn.OpenAsync(cancellationToken);
            return;
        }

        if (conn.State.HasFlag(ConnectionState.Broken))
        {
            await conn.SafeCloseAsync();
            await conn.OpenAsync(cancellationToken);
            return;
        }
    }

    /// <summary>
    /// It closes the connection to the database
    /// </summary>
    public static void SafeClose(this SqlConnection conn)
    {
        if (conn.State.HasFlag(ConnectionState.Closed))
        {
            return;
        }

        conn.Close();
    }

    /// <summary>
    /// Asynchronously closes the connection to the database.
    /// </summary>
    /// <returns>A System.Threading.Tasks.Task representing the asynchronous operation.</returns>
    public static async Task SafeCloseAsync(this SqlConnection conn)
    {
        if (conn.State.HasFlag(ConnectionState.Closed))
        {
            return;
        }

        await conn.CloseAsync();
    }

    public static Func<SqlDataReader, T> MapperFunc<T>(
        Dictionary<string, Func<object, object>>? transformations = null,
        StringComparison fieldsComparison = StringComparison.OrdinalIgnoreCase) where T : class, new()
    {
        return reader =>
        {
            var accessor = TypeMapper.Map(typeof(T));
            var members = accessor.TypeTypeProperties;
            var t = new T();

            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.IsDBNull(i))
                {
                    continue;
                }

                var typeMemberName =
                    members.FirstOrDefault(m => string.Equals(m.Name, reader.GetName(i), fieldsComparison));
                if (typeMemberName == null)
                {
                    continue;
                }

                var ts = transformations ?? new Dictionary<string, Func<object, object>>();
                if (ts.TryGetValue(typeMemberName.Name, out var transformation) == false)
                {
                    transformation = obj => obj;
                }


                accessor[t, typeMemberName.Name] = transformation(reader.GetValue(i));
            }

            return t;
        };
    }

    public static T ExecuteScalar<T>(this SqlConnection conn, string sql,
        Func<object?, T> lambda,
        Dictionary<string, object>? parameters = null)
    {
        conn.SafeOpen();
        using var command = conn.CreateCommand();
        command.CommandText = sql;

        if (parameters.IsNullOrEmpty() == false)
        {
            command.Parameters.AddRange(parameters.ToDbParameter(command));
        }

        var input = command.ExecuteScalar();

        return lambda(input);
    }

    public static T? ExecuteScalar<T>(this SqlConnection conn, string sql,
        Dictionary<string, object>? parameters = null)
    {
        return conn.ExecuteScalar(sql, input =>
        {
            if (input is T inputT)
            {
                return inputT;
            }

            try
            {
                return (T?)Convert.ChangeType(input, typeof(T));
            }
            catch (Exception)
            {
                return default;
            }
        }, parameters);
    }

    public static async Task<T> ExecuteScalarAsync<T>(this SqlConnection conn, string sql,
        Func<object?, T> lambda,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        await conn.SafeOpenAsync(cancellationToken: cancellationToken);
        await using var command = conn.CreateCommand();
        command.CommandText = sql;

        if (parameters.IsNullOrEmpty() == false)
        {
            command.Parameters.AddRange(parameters.ToDbParameter(command));
        }

        var input = await command.ExecuteScalarAsync(cancellationToken);

        return lambda(input);
    }

    public static async Task<T?> ExecuteScalarAsync<T>(this SqlConnection conn, string sql,
        Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        return await conn.ExecuteScalarAsync(sql, input =>
        {
            if (input is T inputT)
            {
                return inputT;
            }

            try
            {
                return (T?)Convert.ChangeType(input, typeof(T));
            }
            catch (Exception)
            {
                return default;
            }
        }, parameters ?? new Dictionary<string, object>(), cancellationToken: cancellationToken);
    }
}