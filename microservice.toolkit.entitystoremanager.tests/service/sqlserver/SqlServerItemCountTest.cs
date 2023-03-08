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

    [Test]
    public async Task Run_PropertiesSearch()
    {
        var response = await this.service.Run(new ItemCountRequest
        {
            Filters = new IWhere[]
            {
                new Where {Key = nameof(MyItem.IntValue), Value = 1},
                new Where {Key = nameof(MyItem.LongValue), Value = 1L},
            }
        });

        Assert.AreEqual(5, response.Payload.Counter);
    }

    [Test]
    public async Task Run_PropertiesIns()
    {
        var response = await this.service.Run(new ItemCountRequest
        {
            Filters = new IWhere[]
            {
                new OrWhere
                {
                    Conditions = new IWhere[]
                    {
                        new Where {Key = nameof(MyItem.IntValue), Value = 1},
                        new Where {Key = nameof(MyItem.FloatValue), Value = 2F},
                    }
                }
            }
        });

        Assert.AreEqual(13, response.Payload.Counter);
    }

    [SetUp]
    public async Task SetUp()
    {
        this.service = new SqlServerItemCount<MyItem>(this.DbConnection);

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