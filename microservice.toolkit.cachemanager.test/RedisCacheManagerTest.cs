﻿
using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager.test;

[ExcludeFromCodeCoverage]
public class RedisCacheManagerTest
{
    private RedisCacheManager manager;

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
    public void SetUp()
    {
        this.manager = new RedisCacheManager("localhost:6379", new NullLogger<RedisCacheManager>());
    }
    #endregion
}
