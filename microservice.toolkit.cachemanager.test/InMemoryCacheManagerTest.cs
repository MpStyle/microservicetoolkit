using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager.test;

[ExcludeFromCodeCoverage]
public class InMemoryCacheManagerTest
{
    private InMemoryCacheManager manager;

    [Test]
    public async Task SetAndRetrieve_KeyValue()
    {
        var setResponse = await this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeMilliseconds());

        Assert.That(setResponse, Is.True);

        var getResponse = await this.manager.Get<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));
    }

    [Test]
    public async Task SetAndRetrieve_KeyValueWithoutExpiration()
    {
        var setResponse = await this.manager.Set("my_key", "my_value");

        Assert.That(setResponse, Is.True);

        var getResponse = await this.manager.Get<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));
    }

    [Test]
    public async Task SetAndRetrieve_ExpiredKeyValue()
    {
        var setResponse = await this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds());

        Assert.That(setResponse, Is.True);

        await Task.Delay(5000);

        var getResponse = await this.manager.Get<string>("my_key");

        Assert.That(getResponse, Is.Null);
    }

    [Test]
    public async Task SetAndRetrieve_UpdateWithNegativeIssuedAt()
    {
        var setResponse = await this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(2).ToUnixTimeMilliseconds());

        Assert.That(setResponse, Is.True);

        var getResponse = await this.manager.Get<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));

        setResponse = await this.manager.Set("my_key", "my_value", DateTimeOffset.UtcNow.AddSeconds(-2).ToUnixTimeMilliseconds());

        Assert.That(setResponse, Is.False);

        getResponse = await this.manager.Get<string>("my_key");

        Assert.That(getResponse, Is.Null);
    }

    [Test]
    public async Task Delete()
    {
        var setResponse = await this.manager.Set("my_key", "my_value");

        Assert.That(setResponse, Is.True);

        var getResponse = await this.manager.Get<string>("my_key");

        Assert.That("my_value", Is.EqualTo(getResponse));

        var deleteResponse = await this.manager.Delete("my_key");

        Assert.That(deleteResponse, Is.True);

        getResponse = await this.manager.Get<string>("my_key");

        Assert.That(getResponse, Is.Null);
    }

    #region SetUp & TearDown
    [SetUp]
    public void SetUp()
    {
        this.manager = new InMemoryCacheManager();
    }
    #endregion
}
