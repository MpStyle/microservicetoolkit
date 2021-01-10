using Microsoft.Data.Sqlite;

using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.connectionmanager
{
    public class SQLiteConnectionManager : IConnectionManager
    {
        private readonly string connectionString;

        public SQLiteConnectionManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public T Execute<T>(Func<DbCommand, T> lambda)
        {
            using (var connection = new SqliteConnection(this.connectionString))
            {
                connection.Open();
                using (var cmd = this.GetCommand(connection))
                {
                    return lambda(cmd);
                }
            }
        }

        private DbCommand GetCommand(DbConnection connection)
        {
            return new SqliteCommand
            {
                Connection = (SqliteConnection)connection
            };
        }

        public DbParameter GetParameter<T>(string name, T value)
        {
            return new SqliteParameter
            {
                ParameterName = name,
                Value = value
            };
        }

        public async Task<T> ExecuteAsync<T>(Func<DbCommand, Task<T>> lambda)
        {
            using (var connection = new SqliteConnection(this.connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = this.GetCommand(connection))
                {
                    return await lambda(cmd);
                }
            }
        }
    }
}
