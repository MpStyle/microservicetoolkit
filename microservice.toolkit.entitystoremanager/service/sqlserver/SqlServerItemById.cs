using microservice.toolkit.connectionmanager;
using microservice.toolkit.core.entity;
using microservice.toolkit.core.extension;
using microservice.toolkit.entitystoremanager.book;
using microservice.toolkit.entitystoremanager.entity;
using microservice.toolkit.entitystoremanager.entity.service;
using microservice.toolkit.entitystoremanager.extension;
using microservice.toolkit.messagemediator;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace microservice.toolkit.entitystoremanager.service.sqlserver;

public class SqlServerItemById<TSource> : Service<ItemByIdRequest, ItemByIdResponse<TSource>>
    where TSource : IItem, new()
{
    private readonly DbConnection connectionManager;

    public SqlServerItemById(DbConnection connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    public override async Task<ServiceResponse<ItemByIdResponse<TSource>>> Run(ItemByIdRequest request)
    {
        if (request.ItemId.IsNullOrEmpty())
        {
            return this.UnsuccessfulResponse(EntityError.ItemByIdInvalidRequest);
        }

        var where = new List<string>
        {
            $"{Item.Id} = @{nameof(request.ItemId)}",
            $"{Item.Type} = @SourceType",
            $"{Item.Enabled} = 1"
        };
        var parameters = new Dictionary<string, object>
        {
            {$"@{nameof(request.ItemId)}", request.ItemId},
            {"@SourceType", typeof(TSource).GetItemName()},
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

        var itemSql = $@"SELECT 
                    {string.Join(", ", select)} 
                FROM {nameof(Item)} 
                WHERE {string.Join(" AND ", where)}";

        var source = await this.connectionManager.ExecuteFirstAsync(itemSql, reader => ItemBuilder.Build<TSource>(
            reader.GetString(0),
            request.ReturnOnlyId == false ? reader.GetBoolean(2) : null,
            request.ReturnOnlyId == false ? reader.GetInt64(3) : null,
            request.ReturnOnlyId == false ? reader.GetInt64(4) : null,
            request.ReturnOnlyId == false ? reader.GetString(5) : null
        ), parameters);

        if (source != null && request.ReturnOnlyId == false)
        {
            var itemPropertiesSql = $@"SELECT 
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
                    var propertyName = reader.GetString(0);
                    var value = (reader.IsDBNull(1) ? null : reader.GetString(1))
                                ?? (object) (reader.IsDBNull(2) ? null : reader.GetInt32(2))
                                ?? (object) (reader.IsDBNull(3) ? null : reader.GetInt64(3))
                                ?? (object) (reader.IsDBNull(4) ? null : Convert.ToSingle(reader.GetDouble(4)))
                                ?? (reader.IsDBNull(5) ? null : reader.GetBoolean(5));
                    var order = reader.GetInt32(6);

                    source.SetValue(propertyName, value, order);

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