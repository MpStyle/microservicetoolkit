using microservice.toolkit.migrationmanager.extension;

using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace microservice.toolkit.entitystoremanager.tests;

[ExcludeFromCodeCoverage]
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
        this.ApplyMigrations();
    }

    private void ApplyMigrations()
    {
        var execDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location);
        var result = this.DbConnection.Apply(
            Path.Combine(execDir, "migration", "microservice.toolkit.entitystoremanager", "sqlserver"),
            ".sql");

        if (result.Exception != null)
        {
            throw result.Exception;
        }
    }
}