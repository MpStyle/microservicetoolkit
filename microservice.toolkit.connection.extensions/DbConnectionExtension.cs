using microservice.toolkit.connection.extensions.objectmapper;
using microservice.toolkit.core.extension;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace microservice.toolkit.connection.extensions;

public static class DbConnectionExtension
{
    public static DbParameter ToDbParameter(this DbCommand command, string name, object? value)
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

    public static int ExecuteStoredProcedure(this DbConnection conn, string storedProcedureName,
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

    public static T Execute<T>(this DbConnection conn, Func<DbCommand, T> lambda)
    {
        conn.SafeOpen();
        using var cmd = conn.CreateCommand();
        return lambda(cmd);
    }

    public static T[] Execute<T>(this DbConnection conn, string sql, Func<DbDataReader, T> lambda,
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

    public static T? ExecuteFirst<T>(this DbConnection conn, string sql, Func<DbDataReader, T> lambda,
        Dictionary<string, object>? parameters = null)
    {
        var result = conn.Execute(sql, lambda, parameters);

        return result.IsNullOrEmpty() ? default : result.First();
    }

    public static async Task<int> ExecuteStoredProcedureAsync(this DbConnection conn, string storedProcedureName,
        Dictionary<string, object>? parameters = null)
    {
        await conn.SafeOpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = storedProcedureName;

        cmd.CommandType = CommandType.StoredProcedure;

        if (parameters.IsNullOrEmpty() == false)
        {
            var values = parameters.ToDbParameter(cmd);
            cmd.Parameters.AddRange(values);
        }

        return await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<T> ExecuteAsync<T>(this DbConnection conn, Func<DbCommand, Task<T>> lambda)
    {
        await conn.SafeOpenAsync();
        await using var cmd = conn.CreateCommand();
        return await lambda(cmd);
    }

    public static async Task<T[]> ExecuteAsync<T>(this DbConnection conn, string sql,
        Func<DbDataReader, T> lambda,
        Dictionary<string, object>? parameters = null)
    {
        return await conn.ExecuteAsync(async command =>
        {
            command.CommandText = sql;

            if (parameters.IsNullOrEmpty() == false)
            {
                command.Parameters.AddRange(parameters.ToDbParameter(command));
            }

            await using var reader = await command.ExecuteReaderAsync();
            var objects = new List<T>();

            while (await reader.ReadAsync())
            {
                objects.Add(lambda(reader));
            }

            return objects.ToArray();
        });
    }

    public static async Task<T[]> ExecuteAsync<T>(this DbConnection conn, string sql,
        Dictionary<string, object>? parameters = null) where T : class, new()
    {
        return await conn.ExecuteAsync(sql, MapperFunc<T>(), parameters);
    }

    public static async Task<T?> ExecuteFirstAsync<T>(this DbConnection conn, string sql,
        Func<DbDataReader, T> lambda,
        Dictionary<string, object>? parameters = null)
    {
        var result = await conn.ExecuteAsync(sql, lambda, parameters);

        return result.IsNullOrEmpty() ? default : result.First();
    }

    public static async Task<T?> ExecuteFirstAsync<T>(this DbConnection conn, string sql,
        Dictionary<string, object>? parameters = null) where T : class, new()
    {
        return await conn.ExecuteFirstAsync(sql, MapperFunc<T>(), parameters);
    }

    public static int ExecuteNonQuery(this DbConnection conn, string query,
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

    public static async Task<int> ExecuteNonQueryAsync(this DbConnection conn, string query,
        Dictionary<string, object>? parameters = null)
    {
        await conn.SafeOpenAsync();
        await using var command = conn.CreateCommand();
        command.CommandText = query;

        if (parameters.IsNullOrEmpty() == false)
        {
            command.Parameters.AddRange(parameters.ToDbParameter(command));
        }

        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// It opens a database connection.
    /// </summary>
    public static void SafeOpen(this DbConnection conn)
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
        }
    }

    /// <summary>
    /// An asynchronous version of microservice.toolkit.connection.extensions.DbConnectionExtensions.Open, which opens
    /// a database connection.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SafeOpenAsync(this DbConnection conn)
    {
        if (conn.State.HasFlag(ConnectionState.Open))
        {
            return;
        }

        if (conn.State.HasFlag(ConnectionState.Closed))
        {
            await conn.OpenAsync();
            return;
        }

        if (conn.State.HasFlag(ConnectionState.Broken))
        {
            await conn.SafeCloseAsync();
            await conn.OpenAsync();
        }
    }

    /// <summary>
    /// It closes the connection to the database
    /// </summary>
    public static void SafeClose(this DbConnection conn)
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
    public static async Task SafeCloseAsync(this DbConnection conn)
    {
        if (conn.State.HasFlag(ConnectionState.Closed))
        {
            return;
        }

        await conn.CloseAsync();
    }

    public static Func<DbDataReader, T> MapperFunc<T>(
        Dictionary<string, Func<object, object>>? transformations = null,
        StringComparison fieldsComparison = StringComparison.OrdinalIgnoreCase) where T : class, new()
    {
        return reader =>
        {
            var accessor = objectmapper.TypeMapper.Map(typeof(T));
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
    
    public static T ExecuteScalar<T>(this DbConnection conn, string sql,
        Func<object, T> lambda,
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

    public static T? ExecuteScalar<T>(this DbConnection conn, string sql,
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
                return (T)Convert.ChangeType(input, typeof(T));
            }
            catch (Exception)
            {
                return default;
            }
        }, parameters);
    }

    public static async Task<T> ExecuteScalarAsync<T>(this DbConnection conn, string sql,
        Func<object, T> lambda,
        Dictionary<string, object>? parameters = null)
    {
        await conn.SafeOpenAsync();
        await using var command = conn.CreateCommand();
        command.CommandText = sql;

        if (parameters.IsNullOrEmpty() == false)
        {
            command.Parameters.AddRange(parameters.ToDbParameter(command));
        }

        var input = await command.ExecuteScalarAsync();

        return lambda(input);
    }

    public static async Task<T?> ExecuteScalarAsync<T>(this DbConnection conn, string sql,
        Dictionary<string, object>? parameters = null) 
    {
        return await conn.ExecuteScalarAsync(sql, input =>
        {
            if (input is T inputT)
            {
                return inputT;
            }
        
            try
            {
                return (T)Convert.ChangeType(input, typeof(T));
            }
            catch (Exception)
            {
                return default;
            }
        }, parameters);
    }
}