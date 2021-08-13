
using mpstyle.microservice.toolkit.book.cachemanager;
using mpstyle.microservice.toolkit.book.connectionmanager;

using System;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Xunit;

namespace mpstyle.microservice.toolkit.test.book.cachemanager
{
    public class SQLiteCacheManagerTest : IAsyncLifetime
    {
        private SQLiteCacheManager manager;

        [Fact]
        public async Task SetAndRetrieve_KeyValue()
        {
            var setResponse = await this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeMilliseconds());

            Assert.True(setResponse);

            var getResponse = await this.manager.Get("my_key");

            Assert.Equal("my_value", getResponse);
        }

        [Fact]
        public async Task SetAndRetrieve_ExpiredKeyValue()
        {
            var setResponse = await this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds());

            Assert.True(setResponse);

            await Task.Delay(5000);

            var getResponse = await this.manager.Get("my_key");

            Assert.Null(getResponse);
        }

        [Fact]
        public async Task SetAndRetrieve_UpdateWithNegativeIssuedAt()
        {
            var setResponse = await this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds());

            Assert.True(setResponse);

            var getResponse = await this.manager.Get("my_key");

            Assert.Equal("my_value", getResponse);

            setResponse = await this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(-2).ToUnixTimeMilliseconds());

            Assert.False(setResponse);

            getResponse = await this.manager.Get("my_key");

            Assert.Null(getResponse);
        }

        #region SetUp & TearDown
        public async Task InitializeAsync()
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
                await this.DisposeAsync();
            }
        }

        public Task DisposeAsync()
        {
            File.Delete("hello.db");
            return Task.CompletedTask;
        }
        #endregion
    }
}
