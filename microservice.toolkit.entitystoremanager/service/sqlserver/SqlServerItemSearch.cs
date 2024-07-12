using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
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

public class SqlServerItemSearch<TSource> : Service<ItemSearchRequest, ItemSearchResponse<TSource>>
    where TSource : IItem, new()
{
    private readonly DbConnection connectionManager;

    public SqlServerItemSearch(DbConnection connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    public override async Task<ServiceResponse<ItemSearchResponse<TSource>>> Run(ItemSearchRequest request)
    {
        var itemType = typeof(TSource);
        var where = new List<string>();
        var parameters = new Dictionary<string, object>();
        var orderBy = new List<string>();

        if (request.IncludeDisabled == false)
        {
            where.Add($"{itemType.GetItemSqlTable()}.{nameof(IItem.Enabled)} = 1");
        }

        if (request.Filters != null)
        {
            var condition = request.Filters.ToSqlServerCondition<TSource>(itemType.GetItemPropertySqlTable());
            where.Add($@"EXISTS(
                        SELECT 1 
                        FROM {itemType.GetItemPropertySqlTable()} 
                        WHERE {itemType.GetItemSqlTable()}.{nameof(IItem.Id)} = {itemType.GetItemPropertySqlTable()}.{TableFieldName.ItemProperty.ItemId}
                            AND {condition.Condition}
                    )");
            
            foreach (var param in condition.Parameters)
            {
                parameters.Add(param.Key, param.Value);
            }
        }

        if (request.Id.IsNullOrEmpty() == false)
        {
            where.Add($"{nameof(IItem.Id)} = @{nameof(request.Id)}");
            parameters.Add($"@{nameof(request.Id)}", request.Id);
        }

        if (request.ExcludeId.IsNullOrEmpty() == false)
        {
            where.Add($"{nameof(IItem.Id)} <> @{nameof(request.ExcludeId)}");
            parameters.Add($"@{nameof(request.ExcludeId)}", request.ExcludeId);
        }

        if (request.Ids.IsNullOrEmpty() == false)
        {
            var parameterNames = new List<string>();
            for (var i = 0; i < request.Ids.Length; i++)
            {
                var parameterName = $"@{nameof(request.Ids)}_{i}";

                parameterNames.Add(parameterName);
                parameters.Add(parameterName, request.Ids[i]);
            }

            where.Add($"{itemType.GetItemSqlTable()}.{nameof(IItem.Id)} IN ({string.Join(",", parameterNames)})");
        }

        where.Add($"{TableFieldName.Item.Type} = @Type");
        parameters.Add("@Type", itemType.GetItemName());

        if (request.OrderBy.IsNullOrEmpty() == false)
        {
            orderBy.AddRange(request.OrderBy.Select(clause => $"c.{clause.Field} {clause.Order ?? "ASC"}"));
        }

        var itemsSql = $@"SELECT 
                    {nameof(IItem.Id)}, 
                    {TableFieldName.Item.Type}, 
                    {nameof(IItem.Enabled)}, 
                    {nameof(IItem.Inserted)}, 
                    {nameof(IItem.Updated)},
                    {nameof(IItem.Updater)}
                FROM {itemType.GetItemSqlTable()} 
                WHERE {string.Join(" AND ", where)}";

        if (orderBy.Any())
        {
            itemsSql += $" ORDER BY {string.Join(", ", orderBy)}";
        }
        else
        {
            itemsSql += $" ORDER BY {nameof(IItem.Id)}";
        }

        if (request.Page.HasValue && request.PageSize.HasValue)
        {
            itemsSql += $" OFFSET {(request.Page) * request.PageSize} ROWS";
            itemsSql += $" FETCH NEXT {request.PageSize} ROWS ONLY";
        }

        var sourceByIds = new Dictionary<string, TSource>();
        await this.connectionManager.ExecuteAsync(itemsSql, reader =>
        {
            var id = reader.GetString(0);
            var source = ItemBuilder.Build<TSource>(
                id,
                request.ReturnOnlyId == false ? reader.GetBoolean(2) : null,
                request.ReturnOnlyId == false ? reader.GetInt64(3) : null,
                request.ReturnOnlyId == false ? reader.GetInt64(4) : null,
                reader.IsDBNull(5) ? null : reader.GetString(5)
            );

            sourceByIds.Add(id, source);

            return source;
        }, parameters);

        if (sourceByIds.IsNullOrEmpty() == false && request.ReturnOnlyId == false)
        {
            var itemIdKeys = new List<string>();
            var itemIdsParameters = new Dictionary<string, object>();

            for (var k = 0; k < sourceByIds.Count; k++)
            {
                itemIdKeys.Add($"@idd{k}");
                itemIdsParameters.Add($"@idd{k}", sourceByIds.Keys.ElementAt(k));
            }

            var itemPropertiesSql = $@"SELECT
                        {TableFieldName.ItemProperty.ItemId}, 
                        [{TableFieldName.ItemProperty.Key}], 
                        {TableFieldName.ItemProperty.StringValue}, 
                        {TableFieldName.ItemProperty.IntValue}, 
                        {TableFieldName.ItemProperty.LongValue}, 
                        {TableFieldName.ItemProperty.FloatValue}, 
                        {TableFieldName.ItemProperty.BoolValue}, 
                        [{TableFieldName.ItemProperty.Order}] 
                    FROM {itemType.GetItemPropertySqlTable()} 
                    WHERE {TableFieldName.ItemProperty.ItemId} IN ({string.Join(",", itemIdKeys)})";

            await this.connectionManager.ExecuteAsync(itemPropertiesSql, reader =>
            {
                var itemId = reader.GetString(0);
                var propertyName = reader.GetString(1);
                var value = (reader.IsDBNull(2) ? null : reader.GetString(2))
                            ?? (object)(reader.IsDBNull(3) ? null : reader.GetInt32(3))
                            ?? (object)(reader.IsDBNull(4) ? null : reader.GetInt64(4))
                            ?? (object)(reader.IsDBNull(5) ? null : Convert.ToSingle(reader.GetDouble(5)))
                            ?? (reader.IsDBNull(6) ? null : reader.GetBoolean(6));
                var order = reader.GetInt32(7);

                if (sourceByIds.ContainsKey(itemId) == false)
                {
                    return default;
                }

                var source = sourceByIds[itemId];

                source.SetValue(propertyName, value, order);

                return source;
            }, itemIdsParameters);
        }

        return this.SuccessfulResponse(new ItemSearchResponse<TSource>
        {
            Items = request.ReturnOnlyId == false ? sourceByIds.Values.ToArray() : null,
            ItemIds = request.ReturnOnlyId ? sourceByIds.Keys.ToArray() : null
        });
    }
}