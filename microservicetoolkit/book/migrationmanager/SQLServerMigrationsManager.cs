
using Microsoft.Extensions.Logging;

using System.Data.SqlClient;

namespace mpstyle.microservice.toolkit.book.migrationmanager
{
    public class SQLServerMigrationsManager : EvolveMigrationManager
    {
        public SQLServerMigrationsManager(ILogger<SQLServerMigrationsManager> logger)
            : base(logger)
        {
        }

        public ApplyResult Apply(SqlMigrationManagerConfiguration configuration)
        {
            return base.ApplyMigration(new MigrationManagerConfiguration
            {
                DbConnection = configuration.DbConnection,
                Extension = configuration.Extension,
                Folder = configuration.Folder,
            });
        }
    }

    public class SqlMigrationManagerConfiguration
    {
        public string Extension { get; set; }
        public string Folder { get; set; }
        public SqlConnection DbConnection { get; set; }
    }
}