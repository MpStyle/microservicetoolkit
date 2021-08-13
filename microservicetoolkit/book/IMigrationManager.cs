using System;
using System.Data.Common;

namespace mpstyle.microservice.toolkit.book
{
    public interface IMigrationManager
    {
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
