
using microservice.toolkit.core;

using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace microservice.toolkit.connectionmanager
{
    public class SQLServerConnectionManager : ConnectionManager
    {
        public SQLServerConnectionManager(string connectionString)
        {
            this.Connection = new SqlConnection(connectionString);
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
            return new SqlCommand
            {
                Connection = (SqlConnection)this.Connection
            };
        }

        public override DbCommand GetCommand(DbConnection connection)
        {
            return new SqlCommand
            {
                Connection = (SqlConnection)connection
            };
        }

        public override DbParameter GetParameter<T>(string name, T value)
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
