using microservice.toolkit.connection.extensions;
using microservice.toolkit.migrationmanager.extension;

using MySqlConnector;

using NUnit.Framework;

using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.migrationmanager.test
{
    [ExcludeFromCodeCoverage]
    public class MigrationManagerTest
    {
        private DbConnection dbConnection;

        [Test]
        public void Apply()
        {
            this.dbConnection.Apply("./data", ".sql");

            var result = this.dbConnection.ExecuteFirst(
                "SELECT * FROM t_user WHERE id = \"admin-01\"",
                reader => new
                {
                    Id = reader.GetString(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2)
                });

            Assert.That("admin-01", Is.EqualTo(result.Id));
            Assert.That("admin", Is.EqualTo(result.Username));
            Assert.That(
                "c7ad44cbad762a5da0a452f9e854fdc1e0e7a52a38015f23f3eab1d80b931dd472634dfac71cd34ebc35d16ab7fb8a90c81f975113d6c7538dc69dd8de9077ec",
                Is.EqualTo(result.Password));
        }

        [SetUp]
        public void Setup()
        {
            var host = Environment.GetEnvironmentVariable("MYSQL_HOST") ?? "127.0.0.1";
            var rootPassword = Environment.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD") ?? "root";
            var database = Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? "microservice_framework_tests";

            this.dbConnection =
                new MySqlConnection($"Server={host};User ID=root;Password={rootPassword};database={database};");
        }

        [TearDown]
        public async Task TearDown()
        {
            await this.dbConnection.ExecuteNonQueryAsync("DROP TABLE IF EXISTS t_user");
            await this.dbConnection.ExecuteNonQueryAsync("DROP TABLE IF EXISTS changelog");
        }
    }
}