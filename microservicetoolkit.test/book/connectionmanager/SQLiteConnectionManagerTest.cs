using mpstyle.microservice.toolkit.book.connectionmanager;

using NUnit.Framework;

using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.test.book.connectionmanager
{
    [ExcludeFromCodeCoverage]
    public class SQLiteConnectionManagerTest
    {
        private SQLiteConnectionManager connectionManager;

        [Test]
        public async Task ExecuteAsync()
        {
            var result = await connectionManager.ExecuteAsync(async (DbCommand cmd) =>
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
            await connectionManager.ExecuteAsync(async (DbCommand cmd) =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        INTEGER PRIMARY KEY,
                        title       TEXT NOT NULL
                    );
                ";
                return await cmd.ExecuteNonQueryAsync();
            });

            Assert.AreEqual(1, await connectionManager.ExecuteNonQueryAsync("INSERT INTO films VALUES (123, 'my_title');"));
        }

        [SetUp]
        public void SetUp()
        {
            this.connectionManager = new SQLiteConnectionManager("Data Source=CacheTest;Mode=Memory;Cache=Shared");
        }

        [TearDown]
        public async Task TearDown()
        {
            await connectionManager.ExecuteNonQueryAsync("DROP TABLE IF EXISTS films");
        }
    }
}
