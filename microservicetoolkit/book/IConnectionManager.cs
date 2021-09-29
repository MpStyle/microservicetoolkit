using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book
{
    /// <summary>
    /// Manager of a SQL database connection.
    /// It Provides utility methods to execute queries.
    /// </summary>
    public interface IConnectionManager
    {
        T Execute<T>(Func<DbCommand, T> lambda);
        Task<T> ExecuteAsync<T>(Func<DbCommand, Task<T>> lambda);

        Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters);

        DbCommand GetCommand();
        DbCommand GetCommand(DbConnection dbConnection);

        DbParameter GetParameter<T>(string name, T value);

        /// <summary>
        /// When overridden in a derived class, opens a database connection.
        /// </summary>
        void Open();

        /// <summary>
        /// An asynchronous version of mpstyle.microservice.toolkit.book.IConnectionManager.Open, which opens
        /// a database connection.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OpenAsync();

        /// <summary>
        /// When overridden in a derived class, closes the connection to the database
        /// </summary>
        void Close();

        /// <summary>
        /// Asynchronously closes the connection to the database.
        /// </summary>
        /// <returns>A System.Threading.Tasks.Task representing the asynchronous operation.</returns>
        Task CloseAsync();
    }

    /// <summary>
    /// Manager of a SQL database connection, which extends mpstyle.microservice.toolkit.book.IConnectionManager
    /// It implements the methods Close, CloseAsync, Open and OpenAsync
    /// </summary>
    public abstract class ConnectionManager : IConnectionManager
    {
        protected DbConnection Connection { get; set; }

        public abstract T Execute<T>(Func<DbCommand, T> lambda);
        public abstract Task<T> ExecuteAsync<T>(Func<DbCommand, Task<T>> lambda);

        public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters = null)
        {
            await this.OpenAsync();
            using (var cmd = this.GetCommand())
            {
                cmd.CommandText = query;

                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        cmd.Parameters.Add(this.GetParameter(parameter.Key, parameter.Value));
                    }
                }

                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public abstract DbParameter GetParameter<T>(string name, T value);

        /// <summary>
        /// It opens a database connection.
        /// </summary>
        public void Open()
        {
            if (this.Connection.State.HasFlag(System.Data.ConnectionState.Open))
            {
                return;
            }

            if (this.Connection.State.HasFlag(System.Data.ConnectionState.Closed))
            {
                this.Connection.Open();
                return;
            }

            if (this.Connection.State.HasFlag(System.Data.ConnectionState.Broken))
            {
                this.Connection.Close();
                this.Connection.Open();
                return;
            }
        }

        /// <summary>
        /// An asynchronous version of mpstyle.microservice.toolkit.book.ConnectionManager.Open, which opens
        /// a database connection.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task OpenAsync()
        {
            if (this.Connection.State.HasFlag(System.Data.ConnectionState.Open))
            {
                return;
            }

            if (this.Connection.State.HasFlag(System.Data.ConnectionState.Closed))
            {
                await this.Connection.OpenAsync();
                return;
            }

            if (this.Connection.State.HasFlag(System.Data.ConnectionState.Broken))
            {
                await this.Connection.CloseAsync();
                await this.Connection.OpenAsync();
                return;
            }
        }

        /// <summary>
        /// It closes the connection to the database
        /// </summary>
        public void Close()
        {
            if (this.Connection.State.HasFlag(System.Data.ConnectionState.Closed))
            {
                return;
            }

            this.Connection.Close();
        }

        /// <summary>
        /// Asynchronously closes the connection to the database.
        /// </summary>
        /// <returns>A System.Threading.Tasks.Task representing the asynchronous operation.</returns>
        public async Task CloseAsync()
        {
            if (this.Connection.State.HasFlag(System.Data.ConnectionState.Closed))
            {
                return;
            }

            await this.Connection.CloseAsync();
        }

        public abstract DbCommand GetCommand();

        public abstract DbCommand GetCommand(DbConnection dbConnection);
    }
}
