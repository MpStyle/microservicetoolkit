
using microservice.toolkit.core;

using Npgsql;

using System;
using System.Data.Common;

namespace microservice.toolkit.connectionmanager
{
    public class PostgreSQLConnectionManager : ConnectionManager
    {
        public PostgreSQLConnectionManager(string connectionString)
        {
            this.Connection = new NpgsqlConnection(connectionString);
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
    }
}
