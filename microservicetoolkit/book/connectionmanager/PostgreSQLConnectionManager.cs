
using Npgsql;

using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.connectionmanager
{
    public class PostgreSQLConnectionManager : ConnectionManager
    {
        public PostgreSQLConnectionManager(string connectionString)
        {
            this.Connection = new NpgsqlConnection(connectionString);
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
            return new NpgsqlCommand
            {
                Connection = (NpgsqlConnection)this.Connection
            };
        }

        public override DbCommand GetCommand(DbConnection connection)
        {
            return new NpgsqlCommand
            {
                Connection = (NpgsqlConnection)connection
            };
        }

        public override DbParameter GetParameter<T>(string name, T value)
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
