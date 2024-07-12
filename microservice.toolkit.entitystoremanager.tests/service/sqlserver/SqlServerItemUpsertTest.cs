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
    private SqlServerItemUpsert<MyCustomItem> customService;

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
            StringsValue = ["my_string_value_01", "my_string_value_02"],
            LongValue = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            LongsValue =
            [
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            ],
            BoolValue = true,
            BooleansValue = [true, false, true],
            FloatValue = 7312.427F,
            FloatsValue =
            [
                7312.427F,
                7312.428F
            ],
            IntValue = 392817,
            IntsValue =
            [
                392817,
                392818
            ],
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

        Assert.That(user.Updater, Is.EqualTo(byIdResponse.Payload.Item.Updater));
        Assert.That(user.Role, Is.EqualTo(byIdResponse.Payload.Item.Role));
        Assert.That(user.StringValue, Is.EqualTo(byIdResponse.Payload.Item.StringValue));
        Assert.That(user.StringsValue[0], Is.EqualTo(byIdResponse.Payload.Item.StringsValue[0]));
        Assert.That(user.StringsValue[1], Is.EqualTo(byIdResponse.Payload.Item.StringsValue[1]));
        Assert.That(user.LongValue, Is.EqualTo(byIdResponse.Payload.Item.LongValue));
        Assert.That(user.LongsValue[0], Is.EqualTo(byIdResponse.Payload.Item.LongsValue[0]));
        Assert.That(user.LongsValue[1], Is.EqualTo(byIdResponse.Payload.Item.LongsValue[1]));
        Assert.That(user.BoolValue, Is.EqualTo(byIdResponse.Payload.Item.BoolValue));
        Assert.That(user.BooleansValue[0], Is.EqualTo(byIdResponse.Payload.Item.BooleansValue[0]));
        Assert.That(user.BooleansValue[1], Is.EqualTo(byIdResponse.Payload.Item.BooleansValue[1]));
        Assert.That(user.FloatValue, Is.EqualTo(byIdResponse.Payload.Item.FloatValue));
        Assert.That(user.FloatsValue[0], Is.EqualTo(byIdResponse.Payload.Item.FloatsValue[0]));
        Assert.That(user.FloatsValue[1], Is.EqualTo(byIdResponse.Payload.Item.FloatsValue[1]));
        Assert.That(user.IntValue, Is.EqualTo(byIdResponse.Payload.Item.IntValue));
        Assert.That(user.IntsValue[0], Is.EqualTo(byIdResponse.Payload.Item.IntsValue[0]));
        Assert.That(user.IntsValue[1], Is.EqualTo(byIdResponse.Payload.Item.IntsValue[1]));
        Assert.That(user.Inserted, Is.EqualTo(byIdResponse.Payload.Item.Inserted));
        Assert.That(user.Updated, Is.EqualTo(byIdResponse.Payload.Item.Updated));
    }

    [Test]
    public async Task Run_Custom()
    {
        var itemId = Guid.NewGuid().ToString();
        var user = new MyCustomItem
        {
            Enabled = true,
            Id = itemId,
            Updater = "me",
            Role = UserRole.SystemAdministrator,
            StringValue = "my_string_value",
            StringsValue = ["my_string_value_01", "my_string_value_02"],
            LongValue = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            LongsValue =
            [
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            ],
            BoolValue = true,
            BooleansValue = [true, false, true],
            FloatValue = 7312.427F,
            FloatsValue =
            [
                7312.427F,
                7312.428F
            ],
            IntValue = 392817,
            IntsValue =
            [
                392817,
                392818
            ],
            Inserted = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Updated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            IgnoredProperty = "random value"
        };
        await this.customService.Run(new ItemUpsertRequest<MyCustomItem>
        {
            Item = user
        });

        var byIdResponse = await new SqlServerItemById<MyCustomItem>(this.DbConnection).Run(new ItemByIdRequest
        {
            ItemId = itemId
        });

        Assert.That(user.Updater, Is.EqualTo(byIdResponse.Payload.Item.Updater));
        Assert.That(user.Role, Is.EqualTo(byIdResponse.Payload.Item.Role));
        Assert.That(user.StringValue, Is.EqualTo(byIdResponse.Payload.Item.StringValue));
        Assert.That(user.StringsValue[0], Is.EqualTo(byIdResponse.Payload.Item.StringsValue[0]));
        Assert.That(user.StringsValue[1], Is.EqualTo(byIdResponse.Payload.Item.StringsValue[1]));
        Assert.That(user.LongValue, Is.EqualTo(byIdResponse.Payload.Item.LongValue));
        Assert.That(user.LongsValue[0], Is.EqualTo(byIdResponse.Payload.Item.LongsValue[0]));
        Assert.That(user.LongsValue[1], Is.EqualTo(byIdResponse.Payload.Item.LongsValue[1]));
        Assert.That(user.BoolValue, Is.EqualTo(byIdResponse.Payload.Item.BoolValue));
        Assert.That(user.BooleansValue[0], Is.EqualTo(byIdResponse.Payload.Item.BooleansValue[0]));
        Assert.That(user.BooleansValue[1], Is.EqualTo(byIdResponse.Payload.Item.BooleansValue[1]));
        Assert.That(user.FloatValue, Is.EqualTo(byIdResponse.Payload.Item.FloatValue));
        Assert.That(user.FloatsValue[0], Is.EqualTo(byIdResponse.Payload.Item.FloatsValue[0]));
        Assert.That(user.FloatsValue[1], Is.EqualTo(byIdResponse.Payload.Item.FloatsValue[1]));
        Assert.That(user.IntValue, Is.EqualTo(byIdResponse.Payload.Item.IntValue));
        Assert.That(user.IntsValue[0], Is.EqualTo(byIdResponse.Payload.Item.IntsValue[0]));
        Assert.That(user.IntsValue[1], Is.EqualTo(byIdResponse.Payload.Item.IntsValue[1]));
        Assert.That(user.Inserted, Is.EqualTo(byIdResponse.Payload.Item.Inserted));
        Assert.That(user.Updated, Is.EqualTo(byIdResponse.Payload.Item.Updated));
        Assert.That("ignored property value", Is.EqualTo(byIdResponse.Payload.Item.IgnoredProperty));
    }

    [SetUp]
    public void SetUp()
    {
        this.service = new SqlServerItemUpsert<MyItem>(this.DbConnection);
        this.customService = new SqlServerItemUpsert<MyCustomItem>(this.DbConnection);
    }

    [TearDown]
    public async Task TearDown()
    {
        await this.DbConnection.ExecuteNonQueryAsync("TRUNCATE TABLE ItemProperty; TRUNCATE TABLE Item;");
        await this.DbConnection.ExecuteNonQueryAsync("TRUNCATE TABLE MyCustomItemProperty; TRUNCATE TABLE MyCustomItem;");
    }
}