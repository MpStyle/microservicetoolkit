using Microsoft.Extensions.Logging;

using MySqlConnector;

namespace mpstyle.microservice.toolkit.book.migrationmanager
{
    public class MySQLMigrationManager : EvolveMigrationManager
    {
        public MySQLMigrationManager(ILogger<MySQLMigrationManager> logger, MigrationManagerConfiguration configuration)
            : base(logger, configuration)
        {
        }

        public ApplyResult Apply(string migrationsFolder)
        {
            return Apply(new MySqlConnection(this.ConnectionString), migrationsFolder);
        }
    }
}