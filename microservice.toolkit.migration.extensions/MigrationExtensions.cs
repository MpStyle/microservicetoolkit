using EvolveDb;

using System;
using System.Data.Common;
using System.IO;

namespace microservice.toolkit.migration.extensions
{
    public static class MigrationExtensions
    {
        public static ApplyResult Apply(this DbConnection connection, string migrationsFolder,
            string migrationFileExtension)
        {
            try
            {
                if (Directory.GetFiles(migrationsFolder, $"*{migrationFileExtension}").Length == 0)
                {
                    throw new Exception("Migration files not found");
                }

                var evolve = new Evolve(connection, Console.WriteLine)
                {
                    Locations = new[] {migrationsFolder},
                    IsEraseDisabled = true,
                    SqlMigrationSuffix = migrationFileExtension
                };

                evolve.Migrate();
            }
            catch (Exception ex)
            {
                return new ApplyResult {Exception = ex};
            }

            return new ApplyResult {Success = true};
        }
    }

    public class ApplyResult
    {
        public Exception Exception { get; set; }
        public bool Success { get; set; }
    }
}