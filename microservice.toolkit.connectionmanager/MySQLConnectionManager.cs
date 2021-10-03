
using microservice.toolkit.core;

using MySqlConnector;

using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace microservice.toolkit.connectionmanager
{
    public class MySQLConnectionManager : ConnectionManager
    {
        public MySQLConnectionManager(string connectionString)
        {
            this.Connection = new MySqlConnection(connectionString);
        }

        public override T Execute<T>(Func<DbCommand, T> lambda)
        {
            this.Open();
            using (var cmd = this.GetCommand())
            {
                return lambda(cmd);
            }
        }

        public override DbCommand GetCommand()
        {
            return new MySqlCommand
            {
                Connection = this.Connection as MySqlConnection
            };
        }

        public override DbCommand GetCommand(DbConnection connection)
        {
            return new MySqlCommand
            {
                Connection = connection as MySqlConnection
            };
        }

        public override DbParameter GetParameter<T>(string name, T value)
        {
            if (value == null)
            {
                return new MySqlParameter
                {
                    ParameterName = name,
                    Value = DBNull.Value
                };
            }

            return new MySqlParameter
            {
                ParameterName = name,
                Value = value
            };
        }

        public override async Task<T> ExecuteAsync<T>(Func<DbCommand, Task<T>> lambda)
        {
            await this.OpenAsync();
            using (var cmd = this.GetCommand())
            {
                return await lambda(cmd);
            }
        }
    }
}
