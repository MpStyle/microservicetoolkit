using System;
using System.Data.Common;

namespace microservice.toolkit.core
{
    /// <summary>
    /// The manager for the database migrations.
    /// </summary>
    public interface IMigrationManager
    {
        /// <summary>
        /// Applies the database migration to the database.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        ApplyResult Apply(MigrationManagerConfiguration configuration);
    }

    public class ApplyResult
    {
        public Exception Exception { get; init; }
        public bool Success { get; init; }
    }

    public class MigrationManagerConfiguration
    {
        public string Extension { get; init; }
        public string Folder { get; init; }
        public DbConnection DbConnection { get; init; }
    }
}
