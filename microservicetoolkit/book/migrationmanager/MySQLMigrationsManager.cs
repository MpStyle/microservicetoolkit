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

        public override ApplyResult Apply(string migrationsFolder)
        {
            return ApplyMigration(new MySqlConnection(this.ConnectionString), migrationsFolder);
        }
    }
}