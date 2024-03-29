using microservice.toolkit.connectionmanager;
using microservice.toolkit.entitystoremanager.entity;
using microservice.toolkit.entitystoremanager.entity.service;
using microservice.toolkit.entitystoremanager.service.sqlserver;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.entitystoremanager.tests.service.sqlserver;

[ExcludeFromCodeCoverage]
public class SqlServerItemByIdTest : MigratedDbTest
{
    private SqlServerItemById<MyItem> service;

    [Test]
    public async Task Run()
    {
        var response = await this.service.Run(new ItemByIdRequest
        {
            ItemId = "my_source_2"
        });

        Assert.That(response.Error.HasValue, Is.False);

        Assert.That("my_string_2_value", Is.EqualTo(response.Payload.Item.StringValue));
    }

    [Test]
    public async Task Run_ReturnOnlyId()
    {
        var response = await this.service.Run(new ItemByIdRequest
        {
            ItemId = "my_source_2",
            ReturnOnlyId = true
        });

        Assert.That(response.Error.HasValue, Is.False);

        Assert.That("my_source_2", Is.EqualTo(response.Payload.ItemId));
        Assert.That(response.Payload.Item, Is.Null);
    }

    [SetUp]
    public async Task SetUp()
    {
        this.service = new SqlServerItemById<MyItem>(this.DbConnection);

        // Test data
        var upsertService = new SqlServerItemUpsert<MyItem>(this.DbConnection);
        for (var i = 0; i < 20; i++)
        {
            await upsertService.Run(new ItemUpsertRequest<MyItem>
            {
                Item = new MyItem
                {
                    Enabled = true,
                    Id = $"my_source_{i + 1}",
                    Updater = "me",
                    Role = UserRole.SystemAdministrator,
                    StringValue = $"my_string_{i + 1}_value",
                    IntValue = 0 % 2,
                }
            });
        }
    }

    [TearDown]
    public async Task TearDown()
    {
        await this.DbConnection.ExecuteNonQueryAsync("TRUNCATE TABLE ItemProperty; TRUNCATE TABLE Item;");
    }
}