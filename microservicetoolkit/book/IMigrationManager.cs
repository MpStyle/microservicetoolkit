using System.Data;
using System;

namespace mpstyle.microservice.toolkit.book
{
    public interface IMigrationManager<T> where T : IDbConnection
    {
        ApplyResult Apply(MigrationManagerConfiguration<T> configuration);
    }

    public class ApplyResult
    {
        public Exception Exception { get; set; }
        public bool Success { get; set; }
    }

    public class MigrationManagerConfiguration<T> where T : IDbConnection
    {
        public string ConnectionString { get; set; }
        public string Extension { get; set; }
        public string Folder { get; set; }
        public T DbConnection { get; set; }
    }
}
