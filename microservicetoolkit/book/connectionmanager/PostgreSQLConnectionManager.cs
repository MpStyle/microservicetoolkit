
using Npgsql;

using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.connectionmanager
{
    public class PostgreSQLConnectionManager : IConnectionManager
    {
        private readonly string connectionString;

        public PostgreSQLConnectionManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public T Execute<T>(Func<DbCommand, T> lambda)
        {
            using (var connection = new NpgsqlConnection(this.connectionString))
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
            return new NpgsqlCommand
            {
                Connection = (NpgsqlConnection)connection
            };
        }

        public DbParameter GetParameter<T>(string name, T value)
        {
            if (value == null)
            {
                return new NpgsqlParameter
                {
                    ParameterName = name,
                    Value = DBNull.Value
                };
            }

            return new NpgsqlParameter
            {
                ParameterName = name,
                Value = value
            };
        }

        public async Task<T> ExecuteAsync<T>(Func<DbCommand, Task<T>> lambda)
        {
            using (var connection = new NpgsqlConnection(this.connectionString))
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
