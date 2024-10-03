﻿using microservice.toolkit.connection.extensions;

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
    private SqliteConnection dbConnection;
    private SQLiteCacheManager cacheManager;

    [Test]
    public async Task SetAsyncAndGetAsync_KeyValue()
    {
        var setResponse = await this.cacheManager.SetAsync("my_key", "my_value", DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeMilliseconds());

        Assert.That(setResponse, Is.True);

        var getResponse = await this.cacheManager.GetAsync<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));
    }
    
    [Test]
    public void SetAndGet_KeyValue()
    {
        var setResponse = this.cacheManager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeMilliseconds());

        Assert.That(setResponse, Is.True);

        var getResponse = this.cacheManager.Get<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));
    }

    [Test]
    public async Task SetAsyncAndGetAsync_KeyValueWithoutExpiration()
    {
        var setResponse = await this.cacheManager.SetAsync("my_key", "my_value");

        Assert.That(setResponse, Is.True);

        var getResponse = await this.cacheManager.GetAsync<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));
    }

    [Test]
    public async Task SetAsyncAndGetAsync_ExpiredKeyValue()
    {
        var setResponse = await this.cacheManager.SetAsync("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds());

        Assert.That(setResponse, Is.True);

        await Task.Delay(5000);

        var getResponse = await this.cacheManager.GetAsync<string>("my_key");

        Assert.That(getResponse, Is.Null);
    }

    [Test]
    public async Task SetAsyncAndGetAsync_UpdateWithNegativeIssuedAt()
    {
        var setResponse = await this.cacheManager.SetAsync("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds());

        Assert.That(setResponse, Is.True);

        var getResponse = await this.cacheManager.GetAsync<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));

        setResponse = await this.cacheManager.SetAsync("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(-2).ToUnixTimeMilliseconds());

        Assert.That(setResponse, Is.False);

        getResponse = await this.cacheManager.GetAsync<string>("my_key");

        Assert.That(getResponse, Is.Null);
    }

    [Test]
    public async Task DeleteAsync()
    {
        var setResponse = await this.cacheManager.SetAsync("my_key", "my_value");

        Assert.That(setResponse, Is.True);

        var getResponse = await this.cacheManager.GetAsync<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));

        var deleteResponse = await this.cacheManager.DeleteAsync("my_key");

        Assert.That(deleteResponse, Is.True);

        getResponse = await this.cacheManager.GetAsync<string>("my_key");

        Assert.That(getResponse, Is.Null);
    }

    [Test]
    public void Delete()
    {
        var setResponse = this.cacheManager.Set("my_key", "my_value");

        Assert.That(setResponse, Is.True);

        var getResponse = this.cacheManager.Get<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));

        var deleteResponse = this.cacheManager.Delete("my_key");

        Assert.That(deleteResponse, Is.True);

        getResponse = this.cacheManager.Get<string>("my_key");

        Assert.That(getResponse, Is.Null);
    }

    #region SetUp & TearDown
    [SetUp]
    public async Task SetUp()
    {
        try
        {
            this.dbConnection = new SqliteConnection("Data Source=CacheTest;Mode=Memory;Cache=Shared");
            const string query = @"
                CREATE TABLE cache(
                    id TEXT PRIMARY KEY,
                    value TEXT NOT NULL,
                    issuedAt INTEGER NOT NULL
                );
            ";
            await this.dbConnection.ExecuteAsync(async cmd =>
            {
                cmd.CommandText = query;
                return await cmd.ExecuteNonQueryAsync();
            });

            this.cacheManager = new SQLiteCacheManager(this.dbConnection);
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
        await this.dbConnection.ExecuteAsync(async cmd =>
        {
            cmd.CommandText = createTableQuery;
            return await cmd.ExecuteNonQueryAsync();
        });
    }
    #endregion
}
