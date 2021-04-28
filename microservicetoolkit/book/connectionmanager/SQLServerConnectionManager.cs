
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.connectionmanager
{
    public class SQLServerConnectionManager : IConnectionManager
    {
        private readonly string connectionString;

        public SQLServerConnectionManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public T Execute<T>(Func<DbCommand, T> lambda)
        {
            using (var connection = new SqlConnection(this.connectionString))
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
            return new SqlCommand
            {
                Connection = (SqlConnection)connection
            };
        }

        public DbParameter GetParameter<T>(string name, T value)
        {
            if (value == null)
            {
                return new SqlParameter
                {
                    ParameterName = name,
                    Value = DBNull.Value
                };
            }

            return new SqlParameter
            {
                ParameterName = name,
                Value = value
            };
        }

        public async Task<T> ExecuteAsync<T>(Func<DbCommand, Task<T>> lambda)
        {
            using (var connection = new SqlConnection(this.connectionString))
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
