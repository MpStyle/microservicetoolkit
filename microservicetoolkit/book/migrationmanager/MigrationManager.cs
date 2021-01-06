using Microsoft.Extensions.Logging;

using System;
using System.IO;

namespace mpstyle.microservice.toolkit.book.migrationmanager
{
    public class MigrationManager : IMigrationManager
    {
        #region Fields
        private readonly ILogger logger;
        #endregion

        public MigrationManager(ILogger logger)
        {
            this.logger = logger;
        }

        public ApplyResult Apply(MigrationManagerConfiguration configuration)
        {
            try
            {
                if (Directory.GetFiles(configuration.Folder, $"*{configuration.Extension}").Length == 0)
                {
                    throw new Exception("Migration files not found");
                }

                var evolve = new Evolve.Evolve(configuration.DbConnection, msg => this.logger.LogInformation(msg))
                {
                    Locations = new[] { configuration.Folder },
                    IsEraseDisabled = true,
                    SqlMigrationSuffix = configuration.Extension
                };

                evolve.Migrate();
            }
            catch (Exception ex)
            {
                this.logger.LogDebug(ex, "Error while applying migrations");
                return new ApplyResult() { Exception = ex };
            }

            return new ApplyResult() { Success = true };
        }
    }
}
