using microservice.toolkit.connectionmanager;

using MySqlConnector;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager.test;

[ExcludeFromCodeCoverage]
public class MysqlCacheManagerTest
{
    private MySqlConnection connectionManager;
    private MysqlCacheManager manager;

    [Test]
    public async Task SetAsyncAndGetAsync_KeyValue()
    {
        var setResponse = await this.manager.SetAsync("my_key", "my_value", DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeMilliseconds());

        Assert.That(setResponse, Is.True);

        var getResponse = await this.manager.GetAsync<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));
    }
    
    [Test]
    public void SetAndGet_KeyValue()
    {
        var setResponse = this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeMilliseconds());

        Assert.That(setResponse, Is.True);

        var getResponse = this.manager.Get<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));
    }

    [Test]
    public async Task SetAsyncAndGetAsync_KeyValueWithoutExpiration()
    {
        var setResponse = await this.manager.SetAsync("my_key", "my_value");

        Assert.That(setResponse, Is.True);

        var getResponse = await this.manager.GetAsync<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));
    }

    [Test]
    public async Task SetAsyncAndGetAsync_ExpiredKeyValue()
    {
        var setResponse = await this.manager.SetAsync("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds());

        Assert.That(setResponse, Is.True);

        await Task.Delay(5000);

        var getResponse = await this.manager.GetAsync<string>("my_key");

        Assert.That(getResponse, Is.Null);
    }

    [Test]
    public async Task SetAsyncAndGetAsync_UpdateWithNegativeIssuedAt()
    {
        var setResponse = await this.manager.SetAsync("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds());

        Assert.That(setResponse, Is.True);

        var getResponse = await this.manager.GetAsync<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));

        setResponse = await this.manager.SetAsync("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(-2).ToUnixTimeMilliseconds());

        Assert.That(setResponse, Is.False);

        getResponse = await this.manager.GetAsync<string>("my_key");

        Assert.That(getResponse, Is.Null);
    }

    [Test]
    public async Task DeleteAsync()
    {
        var setResponse = await this.manager.SetAsync("my_key", "my_value");

        Assert.That(setResponse, Is.True);

        var getResponse = await this.manager.GetAsync<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));

        var deleteResponse = await this.manager.DeleteAsync("my_key");

        Assert.That(deleteResponse, Is.True);

        getResponse = await this.manager.GetAsync<string>("my_key");

        Assert.That(getResponse, Is.Null);
    }
    
    [Test]
    public void Delete()
    {
        var setResponse = this.manager.Set("my_key", "my_value");

        Assert.That(setResponse, Is.True);

        var getResponse = this.manager.Get<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));

        var deleteResponse = this.manager.Delete("my_key");

        Assert.That(deleteResponse, Is.True);

        getResponse = this.manager.Get<string>("my_key");

        Assert.That(getResponse, Is.Null);
    }

    #region SetUp & TearDown
    [SetUp]
    public async Task SetUp()
    {
        var host = Environment.GetEnvironmentVariable("MYSQL_HOST") ?? "127.0.0.1";
        var rootPassword = Environment.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD") ?? "root";
        var database = Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? "microservice_framework_tests";

        this.connectionManager = new MySqlConnection($"Server={host};User ID=root;Password={rootPassword};database={database};SSLMode=Required");
        const string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS cache(
                        id VARCHAR(256) PRIMARY KEY,
                        value TEXT NOT NULL,
                        issuedAt BIGINT NOT NULL
                    );
                ";
        await this.connectionManager.ExecuteAsync(async cmd =>
        {
            cmd.CommandText = createTableQuery;
            return await cmd.ExecuteNonQueryAsync();
        });

        this.manager = new MysqlCacheManager(connectionManager);
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
