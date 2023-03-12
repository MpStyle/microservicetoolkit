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
        var where = new List<string>();
        var parameters = new Dictionary<string, object>();
        var orderBy = new List<string>();

        if (request.IncludeDisabled == false)
        {
            where.Add($"{nameof(Item)}.{Item.Enabled} = 1");
        }

        if (request.Filters != null)
        {
            var condition = request.Filters.ToSqlServerCondition(nameof(ItemProperty));
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

        if (request.Id.IsNullOrEmpty() == false)
        {
            where.Add($"id = @{nameof(request.Id)}");
            parameters.Add($"@{nameof(request.Id)}", request.Id);
        }

        if (request.ExcludeId.IsNullOrEmpty() == false)
        {
            where.Add($"id <> @{nameof(request.ExcludeId)}");
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

            where.Add($"{nameof(Item)}.{Item.Id} IN ({string.Join(",", parameterNames)})");
        }

        where.Add($"{Item.Type} = @Type");
        parameters.Add("@Type", typeof(TSource).Name);

        if (request.OrderBy.IsNullOrEmpty() == false)
        {
            orderBy.AddRange(request.OrderBy.Select(clause => $"c.{clause.Field} {clause.Order ?? "ASC"}"));
        }

        var itemsSql = $@"SELECT 
                    {Item.Id}, 
                    {Item.Type}, 
                    {Item.Enabled}, 
                    {Item.Inserted}, 
                    {Item.Updated},
                    {Item.Updater}
                FROM {nameof(Item)} 
                WHERE {string.Join(" AND ", where)}";

        if (orderBy.Any())
        {
            itemsSql += $" ORDER BY {string.Join(", ", orderBy)}";
        }
        else
        {
            itemsSql += $" ORDER BY {Item.Id}";
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
                        {ItemProperty.Id}, 
                        {ItemProperty.ItemId}, 
                        [{ItemProperty.Key}], 
                        {ItemProperty.StringValue}, 
                        {ItemProperty.IntValue}, 
                        {ItemProperty.LongValue}, 
                        {ItemProperty.FloatValue}, 
                        {ItemProperty.BoolValue}, 
                        [{ItemProperty.Order}] 
                    FROM {nameof(ItemProperty)} 
                    WHERE {ItemProperty.ItemId} IN ({string.Join(",", itemIdKeys)})";

            await this.connectionManager.ExecuteAsync(itemPropertiesSql, reader =>
            {
                var itemId = reader.GetString(1);
                var propertyName = reader.GetString(2);
                var value = (reader.IsDBNull(3) ? null : reader.GetString(3))
                            ?? (object)(reader.IsDBNull(4) ? null : reader.GetInt32(4))
                            ?? (object)(reader.IsDBNull(5) ? null : reader.GetInt64(5))
                            ?? (object)(reader.IsDBNull(6) ? null : Convert.ToSingle(reader.GetDouble(6)))
                            ?? (reader.IsDBNull(7) ? null : reader.GetBoolean(7));
                int? order = reader.IsDBNull(8) ? null : reader.GetInt32(8);

                if (sourceByIds.ContainsKey(itemId) == false)
                {
                    return default;
                }

                var source = sourceByIds[itemId];

                source.Build(propertyName, value, order ?? 0);

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