﻿using microservice.toolkit.core.extension;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace microservice.toolkit.connectionmanager
{
    public static class ConnectionManagerExtension
    {
        private readonly static Dictionary<Type, DbType> TypeMapper = new(37)
        {
            [typeof(byte)] = DbType.Byte,
            [typeof(sbyte)] = DbType.SByte,
            [typeof(short)] = DbType.Int16,
            [typeof(ushort)] = DbType.UInt16,
            [typeof(int)] = DbType.Int32,
            [typeof(uint)] = DbType.UInt32,
            [typeof(long)] = DbType.Int64,
            [typeof(ulong)] = DbType.UInt64,
            [typeof(float)] = DbType.Single,
            [typeof(double)] = DbType.Double,
            [typeof(decimal)] = DbType.Decimal,
            [typeof(bool)] = DbType.Boolean,
            [typeof(string)] = DbType.String,
            [typeof(char)] = DbType.StringFixedLength,
            [typeof(Guid)] = DbType.Guid,
            [typeof(DateTime)] = DbType.DateTime,
            [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
            [typeof(TimeSpan)] = DbType.Time,
            [typeof(byte[])] = DbType.Binary,
            [typeof(byte?)] = DbType.Byte,
            [typeof(sbyte?)] = DbType.SByte,
            [typeof(short?)] = DbType.Int16,
            [typeof(ushort?)] = DbType.UInt16,
            [typeof(int?)] = DbType.Int32,
            [typeof(uint?)] = DbType.UInt32,
            [typeof(long?)] = DbType.Int64,
            [typeof(ulong?)] = DbType.UInt64,
            [typeof(float?)] = DbType.Single,
            [typeof(double?)] = DbType.Double,
            [typeof(decimal?)] = DbType.Decimal,
            [typeof(bool?)] = DbType.Boolean,
            [typeof(char?)] = DbType.StringFixedLength,
            [typeof(Guid?)] = DbType.Guid,
            [typeof(DateTime?)] = DbType.DateTime,
            [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
            [typeof(TimeSpan?)] = DbType.Time,
            [typeof(object)] = DbType.Object
        };

        private static DbParameter[] ToDbParameter(this Dictionary<string, object> obj, DbCommand command)
        {
            if (obj == null)
            {
                return Array.Empty<DbParameter>();
            }

            return obj.Select(item =>
            {
                var param = command.CreateParameter();
                var value = item.Value;

                param.ParameterName = item.Key;
                param.Value = value ?? DBNull.Value;

                var dbType = DbType.Object;
                if (value != null)
                {
                    dbType = value.GetType().IsEnum ? TypeMapper[typeof(int)] : TypeMapper[item.Value.GetType()];
                }

                param.DbType = dbType;

                return param;
            }).ToArray();
        }

        public static T Execute<T>(this DbConnection conn, 
            Func<DbCommand, T> lambda,
            Dictionary<string, object> parameters = null)
        {
            conn.SafeOpen();
            using var cmd = conn.CreateCommand();
            
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters.ToDbParameter(cmd));
            }
            
            return lambda(cmd);
        }

        public static T[] Execute<T>(this DbConnection conn, string sql, Func<DbDataReader, T> lambda,
            Dictionary<string, object> parameters = null)
        {
            return conn.Execute(command =>
            {
                command.CommandText = sql;
                command.Parameters.AddRange(parameters.ToDbParameter(command));

                using (var reader = command.ExecuteReader())
                {
                    var objects = new List<T>();
                    
                    while (reader.Read())
                    {
                        objects.Add(lambda(reader));
                    }

                    return objects.ToArray();
                }
            });
        }

        public static T ExecuteFirst<T>(this DbConnection conn, string sql, Func<DbDataReader, T> lambda,
            Dictionary<string, object> parameters = null)
        {
            var result = conn.Execute(sql, lambda, parameters);

            return result.IsNullOrEmpty() ? default : result.First();
        }

        public static async Task<T> ExecuteAsync<T>(this DbConnection conn, 
            Func<DbCommand, Task<T>> lambda,
            Dictionary<string, object> parameters = null)
        {
            await conn.SafeOpenAsync();
            using (var cmd = conn.CreateCommand())
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters.ToDbParameter(cmd));
                }

                return await lambda(cmd);
            }
        }

        public static async Task<T[]> ExecuteAsync<T>(this DbConnection conn, string sql,
            Func<DbDataReader, T> lambda,
            Dictionary<string, object> parameters = null)
        {
            return await conn.ExecuteAsync(async command =>
            {
                command.CommandText = sql;
                command.Parameters.AddRange(parameters.ToDbParameter(command));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var objects = new List<T>();
                    
                    while (await reader.ReadAsync())
                    {
                        objects.Add(lambda(reader));
                    }

                    return objects.ToArray();
                }
            });
        }

        public static async Task<T> ExecuteFirstAsync<T>(this DbConnection conn, string sql,
            Func<DbDataReader, T> lambda,
            Dictionary<string, object> parameters = null)
        {
            var result = await conn.ExecuteAsync(sql, lambda, parameters);

            return result.IsNullOrEmpty() ? default : result.First();
        }

        public static async Task<int> ExecuteNonQueryAsync(this DbConnection conn, string query,
            Dictionary<string, object> parameters = null)
        {
            await conn.SafeOpenAsync();
            using (var command = conn.CreateCommand())
            {
                command.CommandText = query;
                command.Parameters.AddRange(parameters.ToDbParameter(command));

                return await command.ExecuteNonQueryAsync();
            }
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
                return;
            }
        }

        /// <summary>
        /// An asynchronous version of microservice.toolkit.book.ConnectionManager.Open, which opens
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
                await conn.CloseAsync();
                await conn.OpenAsync();
                return;
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
    }
}