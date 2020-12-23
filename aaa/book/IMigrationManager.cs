using System;

namespace mpstyle.microservice.toolkit.book
{
    public interface IMigrationManager
    {
        ApplyResult Apply(string migrationsFolder);
    }

    public class ApplyResult
    {
        public Exception Exception { get; set; }
        public bool Success { get; set; }
    }
}
