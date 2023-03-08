using entity.sql.tests;

using microservice.toolkit.connectionmanager;
using microservice.toolkit.entitystoremanager.entity.service;
using microservice.toolkit.entitystoremanager.service.sqlserver;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.entitystoremanager.tests.service.sqlserver;

[ExcludeFromCodeCoverage]
public class SqlServerItemUpsertTest : MigratedDbTest
{
    private SqlServerItemUpsert<MyItem> service;

    [Test]
    public async Task Run()
    {
        var itemId = Guid.NewGuid().ToString();
        var user = new MyItem
        {
            Enabled = true,
            Id = itemId,
            Updater = "me",
            Role = UserRole.SystemAdministrator,
            StringValue = "my_string_value",
            StringsValue = new[] {"my_string_value_01", "my_string_value_02"},
            LongValue = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            LongsValue = new[]
            {
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            },
            BoolValue = true,
            BooleansValue = new[] {true, false, true},
            FloatValue = 7312.427F,
            FloatsValue = new[]
            {
                7312.427F,
                7312.428F
            },
            IntValue = 392817,
            IntsValue = new[]
            {
                392817,
                392818
            },
            Inserted = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Updated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };
        await this.service.Run(new ItemUpsertRequest<MyItem>
        {
            Item = user
        });

        var byIdResponse = await new SqlServerItemById<MyItem>(this.DbConnection).Run(new ItemByIdRequest
        {
            ItemId = itemId
        });

        Assert.AreEqual(user.Updater, byIdResponse.Payload.Item.Updater);
        Assert.AreEqual(user.Role, byIdResponse.Payload.Item.Role);
        Assert.AreEqual(user.StringValue, byIdResponse.Payload.Item.StringValue);
        Assert.AreEqual(user.StringsValue[0], byIdResponse.Payload.Item.StringsValue[0]);
        Assert.AreEqual(user.StringsValue[1], byIdResponse.Payload.Item.StringsValue[1]);
        Assert.AreEqual(user.LongValue, byIdResponse.Payload.Item.LongValue);
        Assert.AreEqual(user.LongsValue[0], byIdResponse.Payload.Item.LongsValue[0]);
        Assert.AreEqual(user.LongsValue[1], byIdResponse.Payload.Item.LongsValue[1]);
        Assert.AreEqual(user.BoolValue, byIdResponse.Payload.Item.BoolValue);
        Assert.AreEqual(user.BooleansValue[0], byIdResponse.Payload.Item.BooleansValue[0]);
        Assert.AreEqual(user.BooleansValue[1], byIdResponse.Payload.Item.BooleansValue[1]);
        Assert.AreEqual(user.FloatValue, byIdResponse.Payload.Item.FloatValue);
        Assert.AreEqual(user.FloatsValue[0], byIdResponse.Payload.Item.FloatsValue[0]);
        Assert.AreEqual(user.FloatsValue[1], byIdResponse.Payload.Item.FloatsValue[1]);
        Assert.AreEqual(user.IntValue, byIdResponse.Payload.Item.IntValue);
        Assert.AreEqual(user.IntsValue[0], byIdResponse.Payload.Item.IntsValue[0]);
        Assert.AreEqual(user.IntsValue[1], byIdResponse.Payload.Item.IntsValue[1]);
        Assert.AreEqual(user.Inserted, byIdResponse.Payload.Item.Inserted);
        Assert.AreEqual(user.Updated, byIdResponse.Payload.Item.Updated);
    }

    [SetUp]
    public void SetUp()
    {
        this.service = new SqlServerItemUpsert<MyItem>(this.DbConnection);
    }

    [TearDown]
    public async Task TearDown()
    {
        await this.DbConnection.ExecuteNonQueryAsync("TRUNCATE TABLE ItemProperty; TRUNCATE TABLE Item;");
    }
}