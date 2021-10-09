
using microservice.toolkit.core;

using MySqlConnector;

using System;
using System.Data.Common;

namespace microservice.toolkit.connectionmanager
{
    public class MySQLConnectionManager : ConnectionManager
    {
        public MySQLConnectionManager(string connectionString)
        {
            this.Connection = new MySqlConnection(connectionString);
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
    }
}
