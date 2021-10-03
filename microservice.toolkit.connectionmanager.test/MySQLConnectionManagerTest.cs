
using NUnit.Framework;

using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.connectionmanager.test
{
    [ExcludeFromCodeCoverage]
    public class MySQLConnectionManagerTest
    {
        private MySQLConnectionManager connectionManager;

        [Test]
        public async Task ExecuteAsync()
        {
            var result = await connectionManager.ExecuteAsync(async (DbCommand cmd) =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        char(5) PRIMARY KEY,
                        title       varchar(40) NOT NULL
                    );
                ";
                return await cmd.ExecuteNonQueryAsync();
            });

            Assert.AreEqual(0, result);
        }

        [Test]
        public async Task ExecuteNonQueryAsync()
        {
            await connectionManager.ExecuteAsync(async (DbCommand cmd) =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        char(5) PRIMARY KEY,
                        title       varchar(40) NOT NULL
                    );
                ";
                return await cmd.ExecuteNonQueryAsync();
            });

            Assert.AreEqual(1, await connectionManager.ExecuteNonQueryAsync("INSERT INTO films VALUES ('mycod', 'my_title');"));
        }

        [SetUp]
        public void SetUp()
        {
            var host = Environment.GetEnvironmentVariable("MYSQL_HOST") ?? "127.0.0.1";
            var rootPassword = Environment.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD") ?? "root";
            var database = Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? "microservice_framework_tests";

            this.connectionManager = new MySQLConnectionManager($"Server={host};User ID=root;Password={rootPassword};database={database};");
        }

        [TearDown]
        public async Task TearDown()
        {
            await connectionManager.ExecuteNonQueryAsync("DROP TABLE IF EXISTS films");
        }
    }
}
