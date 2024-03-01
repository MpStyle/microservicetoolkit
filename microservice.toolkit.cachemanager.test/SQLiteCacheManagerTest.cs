using microservice.toolkit.connectionmanager;

using Microsoft.Data.Sqlite;

using NUnit.Framework;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager.test;

[ExcludeFromCodeCoverage]
public class SQLiteCacheManagerTest
{
    private SqliteConnection connectionManager;
    private SQLiteCacheManager manager;

    [Test]
    public async Task SetAndRetrieve_KeyValue()
    {
        var setResponse = await this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeMilliseconds());

        Assert.IsTrue(setResponse);

        var getResponse = await this.manager.Get<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));
    }

    [Test]
    public async Task SetAndRetrieve_KeyValueWithoutExpiration()
    {
        var setResponse = await this.manager.Set("my_key", "my_value");

        Assert.IsTrue(setResponse);

        var getResponse = await this.manager.Get<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));
    }

    [Test]
    public async Task SetAndRetrieve_ExpiredKeyValue()
    {
        var setResponse = await this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds());

        Assert.IsTrue(setResponse);

        await Task.Delay(5000);

        var getResponse = await this.manager.Get<string>("my_key");

        Assert.IsNull(getResponse);
    }

    [Test]
    public async Task SetAndRetrieve_UpdateWithNegativeIssuedAt()
    {
        var setResponse = await this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds());

        Assert.IsTrue(setResponse);

        var getResponse = await this.manager.Get<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));

        setResponse = await this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(-2).ToUnixTimeMilliseconds());

        Assert.IsFalse(setResponse);

        getResponse = await this.manager.Get<string>("my_key");

        Assert.IsNull(getResponse);
    }

    [Test]
    public async Task Delete()
    {
        var setResponse = await this.manager.Set("my_key", "my_value");

        Assert.IsTrue(setResponse);

        var getResponse = await this.manager.Get<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));

        var deleteResponse = await this.manager.Delete("my_key");

        Assert.IsTrue(deleteResponse);

        getResponse = await this.manager.Get<string>("my_key");

        Assert.IsNull(getResponse);
    }

    #region SetUp & TearDown
    [SetUp]
    public async Task SetUp()
    {
        try
        {
            this.connectionManager = new SqliteConnection("Data Source=CacheTest;Mode=Memory;Cache=Shared");
            const string query = @"
                CREATE TABLE cache(
                    id TEXT PRIMARY KEY,
                    value TEXT NOT NULL,
                    issuedAt INTEGER NOT NULL
                );
            ";
            await this.connectionManager.ExecuteAsync(async cmd =>
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
        const string createTableQuery = "DROP TABLE IF EXISTS cache;";
        await this.connectionManager.ExecuteAsync(async cmd =>
        {
            cmd.CommandText = createTableQuery;
            return await cmd.ExecuteNonQueryAsync();
        });
    }
    #endregion
}
