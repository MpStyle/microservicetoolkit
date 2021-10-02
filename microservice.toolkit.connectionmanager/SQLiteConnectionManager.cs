using microservice.toolkit.core;

using Microsoft.Data.Sqlite;

using System;
using System.Data.Common;
using System.Threading.Tasks;

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

        public override T Execute<T>(Func<DbCommand, T> lambda)
        {
            this.Open();
            using (var cmd = this.GetCommand(this.Connection))
            {
                return lambda(cmd);
            }
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

        public override async Task<T> ExecuteAsync<T>(Func<DbCommand, Task<T>> lambda)
        {
            await this.OpenAsync();
            using (var cmd = this.GetCommand(this.Connection))
            {
                return await lambda(cmd);
            }
        }
    }
}
