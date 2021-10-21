using Microsoft.Data.Sqlite;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.connectionmanager.test
{
    [ExcludeFromCodeCoverage]
    public class SQLiteConnectionManagerTest
    {
        private SqliteConnection connectionManager;

        [Test]
        public async Task ExecuteAsync()
        {
            var result = await connectionManager.ExecuteAsync(async cmd =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        INTEGER PRIMARY KEY,
                        title       TEXT NOT NULL
                    );
                ";
                return await cmd.ExecuteNonQueryAsync();
            });

            Assert.AreEqual(0, result);
        }

        [Test]
        public async Task ExecuteNonQueryAsync()
        {
            await connectionManager.ExecuteAsync(async cmd =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        INTEGER PRIMARY KEY,
                        title       TEXT NOT NULL
                    );
                ";
                return await cmd.ExecuteNonQueryAsync();
            });

            Assert.AreEqual(1,
                await connectionManager.ExecuteNonQueryAsync("INSERT INTO films VALUES (123, 'my_title');"));
        }

        [SetUp]
        public void SetUp()
        {
            this.connectionManager = new SqliteConnection("Data Source=CacheTest;Mode=Memory;Cache=Shared");
        }

        [TearDown]
        public async Task TearDown()
        {
            await connectionManager.ExecuteNonQueryAsync("DROP TABLE IF EXISTS films");
            await connectionManager.CloseAsync();
        }
    }
}