
using MySqlConnector;

using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.connectionmanager
{
    public class MySQLConnectionManager : IConnectionManager
    {
        private readonly string connectionString;

        public MySQLConnectionManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public T Execute<T>(Func<DbCommand, T> lambda)
        {
            using (var connection = new MySqlConnection(this.connectionString))
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
            return new MySqlCommand
            {
                Connection = (MySqlConnection)connection
            };
        }

        public DbParameter GetParameter<T>(string name, T value)
        {
            return new MySqlParameter
            {
                ParameterName = name,
                Value = value
            };
        }

        public async Task<T> ExecuteAsync<T>(Func<DbCommand, Task<T>> lambda)
        {
            using (var connection = new MySqlConnection(this.connectionString))
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
