
using Microsoft.Extensions.Logging;

using System.Data.SqlClient;

namespace mpstyle.microservice.toolkit.book.migrationmanager
{
    public class SQLServerMigrationsManager : EvolveMigrationManager<SqlConnection>
    {
        public SQLServerMigrationsManager(ILogger<SQLServerMigrationsManager> logger)
            : base(logger)
        {
        }
    }
}