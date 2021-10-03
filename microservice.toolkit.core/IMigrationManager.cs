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
        public Exception Exception { get; set; }
        public bool Success { get; set; }
    }

    public class MigrationManagerConfiguration
    {
        public string Extension { get; set; }
        public string Folder { get; set; }
        public DbConnection DbConnection { get; set; }
    }
}
