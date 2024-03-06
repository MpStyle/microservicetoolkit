using microservice.toolkit.connectionmanager;
using microservice.toolkit.entitystoremanager.entity;
using microservice.toolkit.entitystoremanager.entity.service;
using microservice.toolkit.entitystoremanager.service.sqlserver;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.entitystoremanager.tests.service.sqlserver;

[ExcludeFromCodeCoverage]
public class SqlServerItemSearchTest : MigratedDbTest
{
    private SqlServerItemSearch<MyItem> service;

    [Test]
    public async Task Run_PropertiesSearch()
    {
        var response = await this.service.Run(new ItemSearchRequest
        {
            Filters = new AndWhere
            {
                Conditions =
                [
                    new Where { Key = nameof(MyItem.IntValue), Value = 1 },
                    new Where { Key = nameof(MyItem.LongValue), Value = 1L },
                ]
            }
        });

        Assert.That(5, Is.EqualTo(response.Payload.Items.Length));
        Assert.That("my_source_10", Is.EqualTo(response.Payload.Items[0].Id));
        Assert.That("my_source_14", Is.EqualTo(response.Payload.Items[1].Id));
        Assert.That("my_source_18", Is.EqualTo(response.Payload.Items[2].Id));
        Assert.That("my_source_2", Is.EqualTo(response.Payload.Items[3].Id));
        Assert.That("my_source_6", Is.EqualTo(response.Payload.Items[4].Id));
    }

    [Test]
    public async Task Run_PropertiesIns()
    {
        var response = await this.service.Run(new ItemSearchRequest
        {
            Filters = new AndWhere
            {
                Conditions =
                [
                    new Where { Key = nameof(MyItem.IntValue), Value = 1 },
                    new OrWhere
                    {
                        Conditions =
                        [
                            new Where { Key = nameof(MyItem.LongValue), Value = 2L },
                            new Where { Key = nameof(MyItem.FloatValue), Value = 2F },
                        ]
                    }
                ]
            }
        });

        Assert.That(response.Error.HasValue, Is.False, "Error code: {0}", response.Error);

        Assert.That(3, Is.EqualTo(response.Payload.Items.Length));
        Assert.That("my_source_12", Is.EqualTo(response.Payload.Items[0].Id));
        Assert.That("my_source_18", Is.EqualTo(response.Payload.Items[1].Id));
        Assert.That("my_source_6", Is.EqualTo(response.Payload.Items[2].Id));
    }

    [Test]
    public async Task Run_IsNullWhere()
    {
        var response = await this.service.Run(new ItemSearchRequest
        {
            Filters = new AndWhere
            {
                Conditions =
                [
                    new IsNullWhere { Key = "NotExistingKey" },
                ]
            }
        });

        Assert.That(20, Is.EqualTo(response.Payload.Items.Length));
    }

    [Test]
    public async Task Run_IsNullWhereAndOtherCondition()
    {
        var response = await this.service.Run(new ItemSearchRequest
        {
            Filters = new AndWhere
            {
                Conditions =
                [
                    new IsNullWhere { Key = "NotExistingKey" },
                    new Where { Key = nameof(MyItem.IntValue), Value = 1 }
                ]
            }
        });

        Assert.That(10, Is.EqualTo(response.Payload.Items.Length));
    }

    [SetUp]
    public async Task SetUp()
    {
        this.service = new SqlServerItemSearch<MyItem>(this.DbConnection);

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
                    IntValue = i % 2,
                    FloatValue = i % 3,
                    LongValue = i % 4,
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