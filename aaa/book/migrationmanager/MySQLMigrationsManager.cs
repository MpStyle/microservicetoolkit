using Microsoft.Extensions.Logging;

using MySqlConnector;

namespace mpstyle.microservice.toolkit.book.migrationmanager
{
    public class MySQLMigrationManager : EvolveMigrationManager
    {
        public MySQLMigrationManager(ILogger<MySQLMigrationManager> logger, IConfigurationManager configurationManager)
            : base(logger, configurationManager)
        {
        }

        public ApplyResult Apply(string migrationsFolder)
        {
            return Apply(new MySqlConnection(this.ConnectionString), migrationsFolder);
        }
    }
}