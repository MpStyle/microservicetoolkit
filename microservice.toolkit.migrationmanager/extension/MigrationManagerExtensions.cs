using System;
using System.Data.Common;
using System.IO;

namespace microservice.toolkit.migrationmanager.extension
{
    public static class MigrationManagerExtensions
    {
        public static ApplyResult Apply(this DbConnection connection, string migrationsFolder,
            string migrationExtension)
        {
            try
            {
                if (Directory.GetFiles(migrationsFolder, $"*{migrationExtension}").Length == 0)
                {
                    throw new Exception("Migration files not found");
                }

                var evolve = new Evolve.Evolve(connection, Console.WriteLine)
                {
                    Locations = new[] { migrationsFolder },
                    IsEraseDisabled = true,
                    SqlMigrationSuffix = migrationExtension
                };

                evolve.Migrate();
            }
            catch (Exception ex)
            {
                return new ApplyResult { Exception = ex };
            }

            return new ApplyResult() { Success = true };
        }
    }
    
    public class ApplyResult
    {
        public Exception Exception { get; set; }
        public bool Success { get; set; }
    }
}