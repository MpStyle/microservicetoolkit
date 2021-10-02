
using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.connectionmanager.test
{
    [ExcludeFromCodeCoverage]
    public class SQLServerConnectionManagerTest
    {
        private SQLServerConnectionManager connectionManager;

        [Test]
        public async Task ExecuteAsync()
        {
            var result = await connectionManager.ExecuteAsync(async cmd =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        char(5) PRIMARY KEY,
                        title       varchar(40) NOT NULL
                    );
                ";
                return await cmd.ExecuteNonQueryAsync();
            });

            Assert.AreEqual(-1, result);
        }

        [Test]
        public async Task ExecuteNonQueryAsync()
        {
            await connectionManager.ExecuteAsync(async cmd =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        char(5) PRIMARY KEY,
                        title       varchar(40) NOT NULL
                    );
                ";
                return await cmd.ExecuteNonQueryAsync();
            });

            Assert.AreEqual(1, await connectionManager.ExecuteNonQueryAsync("INSERT INTO films VALUES ('12345', 'my_title');"));
        }

        [SetUp]
        public void SetUp()
        {
            var host = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "127.0.0.1";
            var port = Environment.GetEnvironmentVariable("SQLSERVER_PORT") ?? "1433";
            var rootPassword = Environment.GetEnvironmentVariable("SQLSERVER_ROOT_PASSWORD") ?? "my_root_password123";
            this.connectionManager = new SQLServerConnectionManager($"Server={host},{port};Database=Master;User Id=SA;Password={rootPassword}");
        }

        [TearDown]
        public async Task TearDown()
        {
            await connectionManager.ExecuteNonQueryAsync("DROP TABLE IF EXISTS films");
        }
    }
}
