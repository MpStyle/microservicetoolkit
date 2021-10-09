
using microservice.toolkit.core;

using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace microservice.toolkit.connectionmanager
{
    public class SQLServerConnectionManager : ConnectionManager
    {
        public SQLServerConnectionManager(string connectionString)
        {
            this.Connection = new SqlConnection(connectionString);
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
    }
}
