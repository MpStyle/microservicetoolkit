using Microsoft.Extensions.Logging;

using System;
using System.Data;
using System.IO;

namespace mpstyle.microservice.toolkit.book.migrationmanager
{
    public abstract class EvolveMigrationManager : IMigrationManager
    {
        #region Fields
        protected ILogger Logger { get; private set; }
        #endregion

        protected EvolveMigrationManager(ILogger logger)
        {
            this.Logger = logger;
        }

        public ApplyResult ApplyMigration(MigrationManagerConfiguration configuration)
        {
            try
            {
                if (Directory.GetFiles(configuration.Folder, $"*{configuration.Extension}").Length == 0)
                {
                    throw new Exception("Migration files not found");
                }

                var evolve = new Evolve.Evolve(configuration.DbConnection, msg => this.Logger.LogInformation(msg))
                {
                    Locations = new[] { configuration.Folder },
                    IsEraseDisabled = true,
                    SqlMigrationSuffix = configuration.Extension
                };

                evolve.Migrate();
            }
            catch (Exception ex)
            {
                this.Logger.LogDebug(ex, "Error");
                return new ApplyResult() { Exception = ex };
            }

            return new ApplyResult() { Success = true };
        }
    }
}
