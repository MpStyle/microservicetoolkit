using Microsoft.Extensions.Logging;

using System;
using System.Data;
using System.IO;

namespace mpstyle.microservice.toolkit.book.migrationmanager
{
    public abstract class EvolveMigrationManager
    {
        #region Fields
        protected ILogger Logger { get; private set; }
        protected string MigrationExtension { get; private set; }
        protected string ConnectionString { get; private set; }
        #endregion

        protected EvolveMigrationManager(ILogger logger, MigrationManagerConfiguration configuration)
        {
            this.Logger = logger;
            this.MigrationExtension = configuration.Extension;
            this.ConnectionString = configuration.ConnectionString;
        }

        protected ApplyResult Apply(IDbConnection dbConnection, string migrationsFolder)
        {
            try
            {
                if (Directory.GetFiles(migrationsFolder, $"*{this.MigrationExtension}").Length == 0)
                {
                    throw new Exception("Migration files not found");
                }

                var evolve = new Evolve.Evolve(dbConnection, msg => this.Logger.LogInformation(msg))
                {
                    Locations = new[] { migrationsFolder },
                    IsEraseDisabled = true,
                    SqlMigrationSuffix = this.MigrationExtension
                };

                evolve.Migrate();
                dbConnection.Close();
            }
            catch (Exception ex)
            {
                this.Logger.LogDebug(ex, "Error");
                return new ApplyResult() { Exception = ex };
            }

            return new ApplyResult() { Success = true };
        }
    }

    public class MigrationManagerConfiguration
    {
        public string ConnectionString { get; set; }
        public string Extension { get; set; }
    }
}
