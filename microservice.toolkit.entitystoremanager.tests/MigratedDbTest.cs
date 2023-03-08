using microservice.toolkit.migrationmanager.extension;

using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;

namespace entity.sql.tests;

public class MigratedDbTest
{
    protected DbConnection DbConnection { get; }

    protected MigratedDbTest()
    {
        var host = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "127.0.0.1";
        var port = Environment.GetEnvironmentVariable("SQLSERVER_PORT") ?? "1433";
        var rootPassword = Environment.GetEnvironmentVariable("SQLSERVER_ROOT_PASSWORD") ?? "my_root_password123";
        this.DbConnection =
            new SqlConnection($"Server={host},{port};Database=master;User Id=SA;Password={rootPassword}");
        var migrationsFolder = Path.Combine(".", "migration", "microservice.toolkit.entitystoremanager", "sqlserver");
        var result = this.DbConnection.Apply(
            migrationsFolder,
            ".sql");
    }
}