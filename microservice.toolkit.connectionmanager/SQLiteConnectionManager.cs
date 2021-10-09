using microservice.toolkit.core;

using Microsoft.Data.Sqlite;

using System;
using System.Data.Common;

namespace microservice.toolkit.connectionmanager
{
    public class SQLiteConnectionManager : ConnectionManager
    {
        /// <summary>
        /// Example of supported connection strings:
        /// - Local file: Data Source=hello.db
        /// - In memoery: Data Source=:memory:
        /// - Shareable in-memory database: Data Source=InMemorySample;Mode=Memory;Cache=Shared
        /// 
        /// For more details about connection string see https://docs.microsoft.com/it-it/dotnet/standard/data/sqlite/connection-strings
        /// </summary>
        /// <param name="connectionString"></param>
        public SQLiteConnectionManager(string connectionString)
        {
            this.Connection = new SqliteConnection(connectionString);
        }

        public override DbCommand GetCommand()
        {
            return new SqliteCommand
            {
                Connection = (SqliteConnection)this.Connection
            };
        }

        public override DbCommand GetCommand(DbConnection connection)
        {
            return new SqliteCommand
            {
                Connection = (SqliteConnection)connection
            };
        }

        public override DbParameter GetParameter<T>(string name, T value)
        {
            if (value == null)
            {
                return new SqliteParameter
                {
                    ParameterName = name,
                    Value = DBNull.Value
                };
            }

            return new SqliteParameter
            {
                ParameterName = name,
                Value = value
            };
        }
    }
}
