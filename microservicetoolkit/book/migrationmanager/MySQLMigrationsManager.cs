using Microsoft.Extensions.Logging;

using MySqlConnector;

namespace mpstyle.microservice.toolkit.book.migrationmanager
{
    public class MySQLMigrationManager : EvolveMigrationManager<MySqlConnection>
    {
        public MySQLMigrationManager(ILogger<MySQLMigrationManager> logger)
            : base(logger)
        {
        }
    }
}