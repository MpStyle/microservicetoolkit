
using mpstyle.microservice.toolkit.book.cachemanager;
using mpstyle.microservice.toolkit.book.connectionmanager;

using NUnit.Framework;

using System;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.test.book.cachemanager
{
    public class SQLiteCacheManagerTest
    {
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

            Assert.True(setResponse);

            var getResponse = await this.manager.Get("my_key");

            Assert.AreEqual("my_value", getResponse);

            var deleteResponse = await this.manager.Delete("my_key");

            Assert.True(deleteResponse);

            getResponse = await this.manager.Get("my_key");

            Assert.IsNull(getResponse);
        }

        #region SetUp & TearDown
        [SetUp]
        public async Task SetUp()
        {
            try
            {
                var connectionManager = new SQLiteConnectionManager("Data Source=hello.db");
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

                this.manager = new SQLiteCacheManager(connectionManager);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        [TearDown]
        public Task TearDown()
        {
            File.Delete("hello.db");
            return Task.CompletedTask;
        }
        #endregion
    }
}
