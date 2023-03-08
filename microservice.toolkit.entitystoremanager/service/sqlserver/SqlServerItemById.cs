using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

using microservice.toolkit.entitystoremanager.extension;
using microservice.toolkit.connectionmanager;
using microservice.toolkit.core.entity;
using microservice.toolkit.core.extension;
using microservice.toolkit.entitystoremanager.book;
using microservice.toolkit.entitystoremanager.entity;
using microservice.toolkit.entitystoremanager.entity.service;
using microservice.toolkit.messagemediator;

namespace microservice.toolkit.entitystoremanager.service.sqlserver;

public class SqlServerItemById<TSource> : Service<ItemByIdRequest, ItemByIdResponse<TSource>>
    where TSource : IItem, new()
{
    private readonly DbConnection connectionManager;
    private readonly ItemBuilder itemBuilder;

    public SqlServerItemById(DbConnection connectionManager)
    {
        this.connectionManager = connectionManager;
        this.itemBuilder = new ItemBuilder();
    }

    public override async Task<ServiceResponse<ItemByIdResponse<TSource>>> Run(ItemByIdRequest request)
    {
        if (request.ItemId.IsNullOrEmpty())
        {
            return this.UnsuccessfulResponse(CoreError.ItemByIdInvalidRequest);
        }

        var objectType = typeof(TSource);
        var where = new List<string>
        {
            $"{Item.Id} = @{nameof(request.ItemId)}",
            $"{Item.Type} = @SourceType",
            $"{Item.Enabled} = 1"
        };
        var parameters = new Dictionary<string, object>
        {
            {$"@{nameof(request.ItemId)}", request.ItemId},
            {"@SourceType", objectType.Name},
        };
        var select = new List<string>
        {
            Item.Id
        };

        if (request.ReturnOnlyId == false)
        {
            select.AddRange(new[]
            {
                Item.Type,
                Item.Enabled,
                Item.Inserted,
                Item.Updated,
                Item.Updater
            });
        }

        if (request.Filters.IsNullOrEmpty() == false)
        {
            foreach (var filter in request.Filters)
            {
                var condition = filter.ToSqlServerCondition(nameof(ItemProperty));
                where.Add($@"EXISTS(
                        SELECT 1 
                        FROM {nameof(ItemProperty)} 
                        WHERE {nameof(Item)}.{Item.Id} = {nameof(ItemProperty)}.{ItemProperty.ItemId}
                            AND {condition.Condition}
                    )");
                foreach (var param in condition.Parameters)
                {
                    parameters.Add(param.Key, param.Value);
                }
            }
        }

        var itemSql = $@"SELECT 
                    {string.Join(", ", select)} 
                FROM {nameof(Item)} 
                WHERE {string.Join(" AND ", where)}";

        var source = await this.connectionManager.ExecuteFirstAsync(itemSql, reader => this.itemBuilder.Build<TSource>(
            reader.GetString(0),
            request.ReturnOnlyId == false ? reader.GetBoolean(2) : null,
            request.ReturnOnlyId == false ? reader.GetInt64(3) : null,
            request.ReturnOnlyId == false ? reader.GetInt64(4) : null,
            request.ReturnOnlyId == false ? reader.GetString(5) : null
        ), parameters);

        if (source != null && request.ReturnOnlyId == false)
        {
            var itemPropertiesSql = $@"SELECT 
                        {ItemProperty.Id}, 
                        [{ItemProperty.Key}], 
                        {ItemProperty.StringValue}, 
                        {ItemProperty.IntValue}, 
                        {ItemProperty.LongValue}, 
                        {ItemProperty.FloatValue}, 
                        {ItemProperty.BoolValue}, 
                        [{ItemProperty.Order}] 
                    FROM {nameof(ItemProperty)} 
                    WHERE {ItemProperty.ItemId} = @ItemId";

            await this.connectionManager.ExecuteAsync(itemPropertiesSql, reader =>
                {
                    var propertyName = reader.GetString(1);
                    var value = (reader.IsDBNull(2) ? null : reader.GetString(2))
                                ?? (object) (reader.IsDBNull(3) ? null : reader.GetInt32(3))
                                ?? (object) (reader.IsDBNull(4) ? null : reader.GetInt64(4))
                                ?? (object) (reader.IsDBNull(5) ? null : Convert.ToSingle(reader.GetDouble(5)))
                                ?? (reader.IsDBNull(6) ? null : reader.GetBoolean(6));
                    int? order = reader.IsDBNull(7) ? null : reader.GetInt32(7);

                    this.itemBuilder.Build(propertyName, value, order ?? 0, ref source);

                    return source;
                },
                new Dictionary<string, object> {{"@ItemId", request.ItemId}});
        }

        if (request.ReturnOnlyId)
        {
            return this.SuccessfulResponse(new ItemByIdResponse<TSource>
            {
                ItemId = source?.Id
            });
        }

        return this.SuccessfulResponse(new ItemByIdResponse<TSource>
        {
            Item = source
        });
    }
}