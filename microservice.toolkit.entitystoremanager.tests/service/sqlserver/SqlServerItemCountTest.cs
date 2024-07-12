using microservice.toolkit.connectionmanager;
using microservice.toolkit.entitystoremanager.entity;
using microservice.toolkit.entitystoremanager.entity.service;
using microservice.toolkit.entitystoremanager.service.sqlserver;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.entitystoremanager.tests.service.sqlserver;

[ExcludeFromCodeCoverage]
public class SqlServerItemCountTest : MigratedDbTest
{
    private SqlServerItemCount<MyItem> service;
    private SqlServerItemCount<MyCustomItem> customService;

    [Test]
    public async Task Run_PropertiesSearch()
    {
        var response = await this.service.Run(new ItemCountRequest
        {
            Filters =
            [
                new Where {Key = nameof(MyItem.IntValue), Value = 1},
                new Where {Key = nameof(MyItem.LongValue), Value = 1L},
            ]
        });

        Assert.That(5, Is.EqualTo(response.Payload.Counter));
    }

    [Test]
    public async Task Run_PropertiesSearch_Custom()
    {
        var response = await this.customService.Run(new ItemCountRequest
        {
            Filters =
            [
                new Where {Key = "int_value", Value = 1},
                new Where {Key = "long_value", Value = 1L},
            ]
        });

        Assert.That(5, Is.EqualTo(response.Payload.Counter));
    }

    [Test]
    public async Task Run_PropertiesIns()
    {
        var response = await this.service.Run(new ItemCountRequest
        {
            Filters =
            [
                new OrWhere
                {
                    Conditions =
                    [
                        new Where {Key = nameof(MyItem.IntValue), Value = 1},
                        new Where {Key = nameof(MyItem.FloatValue), Value = 2F},
                    ]
                }
            ]
        });

        Assert.That(13, Is.EqualTo(response.Payload.Counter));
    }

    [Test]
    public async Task Run_PropertiesIns_Custom()
    {
        var response = await this.customService.Run(new ItemCountRequest
        {
            Filters =
            [
                new OrWhere
                {
                    Conditions =
                    [
                        new Where {Key = "int_value", Value = 1},
                        new Where {Key = "float_value", Value = 2F},
                    ]
                }
            ]
        });

        Assert.That(13, Is.EqualTo(response.Payload.Counter));
    }

    [SetUp]
    public async Task SetUp()
    {
        this.service = new SqlServerItemCount<MyItem>(this.DbConnection);
        this.customService = new SqlServerItemCount<MyCustomItem>(this.DbConnection);

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

        // Test custom data
        var upsertCustomService = new SqlServerItemUpsert<MyCustomItem>(this.DbConnection);
        for (var i = 0; i < 20; i++)
        {
            await upsertCustomService.Run(new ItemUpsertRequest<MyCustomItem>
            {
                Item = new MyCustomItem
                {
                    Enabled = true,
                    Id = $"my_custom_source_{i + 1}",
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