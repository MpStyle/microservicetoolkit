
using Microsoft.Extensions.Logging;

using System.Data.SqlClient;

namespace mpstyle.microservice.toolkit.book.migrationmanager
{
    public class SQLServerMigrationsManager : EvolveMigrationManager
    {
        public SQLServerMigrationsManager(ILogger<SQLServerMigrationsManager> logger, MigrationManagerConfiguration configuration)
            : base(logger, configuration)
        {
        }

        public ApplyResult Apply(string migrationsFolder)
        {
            return Apply(new SqlConnection(this.ConnectionString), migrationsFolder);
        }
    }
}