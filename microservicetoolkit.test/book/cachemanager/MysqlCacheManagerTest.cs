using mpstyle.microservice.toolkit.book.cachemanager;
using mpstyle.microservice.toolkit.book.connectionmanager;

using NUnit.Framework;

using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.test.book.cachemanager
{
    [ExcludeFromCodeCoverage]
    public class MysqlCacheManagerTest
    {
        private const string databaseName = "microservice_framework_tests";
        private MysqlCacheManager manager;

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
            var connectionManager = new MySQLConnectionManager("Server=127.0.0.1;User ID=root;Password=root");
            var createDatabaseQuery = @$"
                    CREATE DATABASE IF NOT EXISTS {databaseName};
                    USE {databaseName};
                ";
            var createDatabase = await connectionManager.ExecuteAsync(async (DbCommand cmd) =>
            {
                cmd.CommandText = createDatabaseQuery;
                return await cmd.ExecuteNonQueryAsync();
            });

            connectionManager = new MySQLConnectionManager($"Server=127.0.0.1;User ID=root;Password=root;database={databaseName};");
            var createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS cache(
                        id VARCHAR(256) PRIMARY KEY,
                        value TEXT NOT NULL,
                        issuedAt BIGINT NOT NULL
                    );
                ";
            var createTable = await connectionManager.ExecuteAsync(async (DbCommand cmd) =>
            {
                cmd.CommandText = createTableQuery;
                return await cmd.ExecuteNonQueryAsync();
            });

            this.manager = new MysqlCacheManager(connectionManager);
        }

        [TearDown]
        public async Task TearDown()
        {
            var connectionManager = new MySQLConnectionManager($"Server=127.0.0.1;User ID=root;Password=root;database={databaseName};");
            var createTableQuery = "DROP TABLE IF EXISTS cache;";
            var createTable = await connectionManager.ExecuteAsync(async (DbCommand cmd) =>
            {
                cmd.CommandText = createTableQuery;
                return await cmd.ExecuteNonQueryAsync();
            });
        }
        #endregion
    }
}
