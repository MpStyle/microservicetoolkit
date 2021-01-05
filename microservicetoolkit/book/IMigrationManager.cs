using System.Data;
using System;

namespace mpstyle.microservice.toolkit.book
{
    public interface IMigrationManager
    {
        ApplyResult ApplyMigration(MigrationManagerConfiguration configuration);
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
        public IDbConnection DbConnection { get; set; }
    }
}
