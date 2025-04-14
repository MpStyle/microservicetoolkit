using Enyim.Caching;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager.test;

[ExcludeFromCodeCoverage]
public class MemcachedCacheManagerTest
{
    private MemcachedCacheManager manager;

    [Test]
    public async Task SetAsyncAndGetAsync_KeyValue()
    {
        var setResponse = await this.manager.SetAsync("my_key", "my_value",
            DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeMilliseconds(), CancellationToken.None);

        Assert.That(setResponse, Is.True);

        var getResponse = await this.manager.GetAsync<string>("my_key", CancellationToken.None);

        Assert.That("my_value", Is.EqualTo(getResponse));
    }

    [Test]
    public void SetAndGet_KeyValue()
    {
        var setResponse = this.manager.Set("my_key", "my_value",
            DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeMilliseconds());

        Assert.That(setResponse, Is.True);

        var getResponse = this.manager.Get<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));
    }

    [Test]
    public async Task SetAsyncAndGetAsync_KeyValueWithoutExpiration()
    {
        var setResponse = await this.manager.SetAsync("my_key", "my_value", CancellationToken.None);

        Assert.That(setResponse, Is.True);

        var getResponse = await this.manager.GetAsync<string>("my_key", CancellationToken.None);

        Assert.That("my_value", Is.EqualTo(getResponse));
    }

    [Test]
    public async Task SetAsyncAndGetAsync_ExpiredKeyValue()
    {
        var setResponse = await this.manager.SetAsync("my_key", "my_value",
            DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds(), CancellationToken.None);

        Assert.That(setResponse, Is.True);

        await Task.Delay(5000);

        var getResponse = await this.manager.GetAsync<string>("my_key", CancellationToken.None);

        Assert.That(getResponse, Is.Null);
    }

    [Test]
    public async Task SetAsyncAndGetAsync_UpdateWithNegativeIssuedAt()
    {
        var setResponse = await this.manager.SetAsync("my_key", "my_value",
            DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds(), CancellationToken.None);

        Assert.That(setResponse, Is.True);

        var getResponse = await this.manager.GetAsync<string>("my_key", CancellationToken.None);

        Assert.That("my_value", Is.EqualTo(getResponse));

        setResponse = await this.manager.SetAsync("my_key", "my_value",
            DateTimeOffset.UtcNow.AddSeconds(-2).ToUnixTimeMilliseconds(), CancellationToken.None);

        Assert.That(setResponse, Is.False);

        getResponse = await this.manager.GetAsync<string>("my_key", CancellationToken.None);

        Assert.That(getResponse, Is.Null);
    }

    [Test]
    public async Task DeleteAsync()
    {
        var setResponse = await this.manager.SetAsync("my_key", "my_value", CancellationToken.None);

        Assert.That(setResponse, Is.True);

        var getResponse = await this.manager.GetAsync<string>("my_key", CancellationToken.None);

        Assert.That("my_value", Is.EqualTo(getResponse));

        var deleteResponse = await this.manager.DeleteAsync("my_key", CancellationToken.None);

        Assert.That(deleteResponse, Is.True);

        getResponse = await this.manager.GetAsync<string>("my_key", CancellationToken.None);

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
    public void SetUp()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddEnyimMemcached(options =>
        {
            options.AddServer("localhost", 11211);
        });

        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Information).AddConsole());
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        this.manager = new MemcachedCacheManager(serviceProvider.GetService<IMemcachedClient>() as MemcachedClient);
    }

    [TearDown]
    public void TearDown()
    {
        this.manager.Dispose(true);
    }

    #endregion
}