using Microsoft.Extensions.Logging;

using MySqlConnector;

namespace mpstyle.microservice.toolkit.book.migrationmanager
{
    public class MySQLMigrationManager : EvolveMigrationManager
    {
        public MySQLMigrationManager(ILogger<MySQLMigrationManager> logger)
            : base(logger)
        {
        }

        public ApplyResult Apply(MySqlMigrationManagerConfiguration configuration)
        {
            return base.ApplyMigration(new MigrationManagerConfiguration
            {
                DbConnection = configuration.DbConnection,
                Extension = configuration.Extension,
                Folder = configuration.Folder,
            });
        }
    }

    public class MySqlMigrationManagerConfiguration
    {
        public string Extension { get; set; }
        public string Folder { get; set; }
        public MySqlConnection DbConnection { get; set; }
    }
}