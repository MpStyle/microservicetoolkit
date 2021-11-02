using microservice.toolkit.connectionmanager;
using microservice.toolkit.core;

using MySqlConnector;

using NUnit.Framework;

using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace microservice.toolkit.migrationmanager.test
{
    public class MigrationManagerTest
    {
        private IMigrationManager manager;
        private DbConnection connectionManager;

        [Test]
        public void Apply()
        {
            this.manager.Apply(new MigrationManagerConfiguration
            {
                DbConnection = this.connectionManager, Extension = ".sql", Folder = "./data"
            });

            var result = this.connectionManager.ExecuteFirst(
                "SELECT * FROM t_user WHERE id = \"admin-01\"",
                reader => new
                {
                    Id = reader.GetString(0), Username = reader.GetString(1), Password = reader.GetString(2)
                });

            Assert.AreEqual("admin-01", result.Id);
            Assert.AreEqual("admin", result.Username);
            Assert.AreEqual(
                "c7ad44cbad762a5da0a452f9e854fdc1e0e7a52a38015f23f3eab1d80b931dd472634dfac71cd34ebc35d16ab7fb8a90c81f975113d6c7538dc69dd8de9077ec",
                result.Password);
        }

        [SetUp]
        public void Setup()
        {
            var host = Environment.GetEnvironmentVariable("MYSQL_HOST") ?? "127.0.0.1";
            var rootPassword = Environment.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD") ?? "root";
            var database = Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? "microservice_framework_tests";

            this.manager = new MigrationManager();
            this.connectionManager =
                new MySqlConnection($"Server={host};User ID=root;Password={rootPassword};database={database};");
        }

        [TearDown]
        public async Task TearDown()
        {
            await connectionManager.ExecuteNonQueryAsync("DROP TABLE IF EXISTS t_user");
            await connectionManager.ExecuteNonQueryAsync("DROP TABLE IF EXISTS changelog");
        }
    }
}