using microservice.toolkit.connectionmanager;

using NUnit.Framework;

using System;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager.test
{
    [ExcludeFromCodeCoverage]
    public class SQLiteCacheManagerTest
    {
        private SQLiteConnectionManager connectionManager;
        private SQLiteCacheManager manager;

        [Test]
        public async Task SetAndRetrieve_KeyValue()
        {
            var setResponse = await this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeMilliseconds());

            Assert.IsTrue(setResponse);

            var getResponse = await this.manager.Get("my_key");

            Assert.AreEqual("my_value", getResponse);
        }

        [Test]
        public async Task SetAndRetrieve_KeyValueWithoutExpiration()
        {
            var setResponse = await this.manager.Set("my_key", "my_value");

            Assert.IsTrue(setResponse);

            var getResponse = await this.manager.Get("my_key");

            Assert.AreEqual("my_value", getResponse);
        }

        [Test]
        public async Task SetAndRetrieve_ExpiredKeyValue()
        {
            var setResponse = await this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds());

            Assert.IsTrue(setResponse);

            await Task.Delay(5000);

            var getResponse = await this.manager.Get("my_key");

            Assert.IsNull(getResponse);
        }

        [Test]
        public async Task SetAndRetrieve_UpdateWithNegativeIssuedAt()
        {
            var setResponse = await this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds());

            Assert.IsTrue(setResponse);

            var getResponse = await this.manager.Get("my_key");

            Assert.AreEqual("my_value", getResponse);

            setResponse = await this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(-2).ToUnixTimeMilliseconds());

            Assert.IsFalse(setResponse);

            getResponse = await this.manager.Get("my_key");

            Assert.IsNull(getResponse);
        }

        [Test]
        public async Task Delete()
        {
            var setResponse = await this.manager.Set("my_key", "my_value");

            Assert.IsTrue(setResponse);

            var getResponse = await this.manager.Get("my_key");

            Assert.AreEqual("my_value", getResponse);

            var deleteResponse = await this.manager.Delete("my_key");

            Assert.IsTrue(deleteResponse);

            getResponse = await this.manager.Get("my_key");

            Assert.IsNull(getResponse);
        }

        #region SetUp & TearDown
        [SetUp]
        public async Task SetUp()
        {
            try
            {
                this.connectionManager = new SQLiteConnectionManager("Data Source=CacheTest;Mode=Memory;Cache=Shared");
                var query = @"
                CREATE TABLE cache(
                    id TEXT PRIMARY KEY,
                    value TEXT NOT NULL,
                    issuedAt INTEGER NOT NULL
                );
            ";
                var createTable = await connectionManager.ExecuteAsync(async (DbCommand cmd) =>
                {
                    cmd.CommandText = query;
                    return await cmd.ExecuteNonQueryAsync();
                });

                this.manager = new SQLiteCacheManager(this.connectionManager);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        [TearDown]
        public async Task TearDown()
        {
            var createTableQuery = "DROP TABLE IF EXISTS cache;";
            var createTable = await this.connectionManager.ExecuteAsync(async (DbCommand cmd) =>
            {
                cmd.CommandText = createTableQuery;
                return await cmd.ExecuteNonQueryAsync();
            });
        }
        #endregion
    }
}
