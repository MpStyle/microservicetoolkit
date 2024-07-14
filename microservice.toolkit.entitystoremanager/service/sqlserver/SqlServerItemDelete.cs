using microservice.toolkit.connectionmanager;
using microservice.toolkit.core.entity;
using microservice.toolkit.core.extension;
using microservice.toolkit.entitystoremanager.book;
using microservice.toolkit.entitystoremanager.entity;
using microservice.toolkit.entitystoremanager.entity.service;
using microservice.toolkit.entitystoremanager.extension;
using microservice.toolkit.messagemediator;

using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace microservice.toolkit.entitystoremanager.service.sqlserver;

public class SqlServerItemDelete<TSource>(DbConnection connectionManager)
    : Service<ItemDeleteRequest, ItemDeleteResponse>
    where TSource : IItem, new()
{
    public override async Task<ServiceResponse<ItemDeleteResponse>> Run(ItemDeleteRequest request)
    {
        var itemType = typeof(TSource);
        var where = new List<string>();
        var parameters = new Dictionary<string, object>();

        if (request.Filters != null)
        {
            var condition = request.Filters.ToSqlServerCondition<TSource>(itemType.GetItemPropertySqlTable());
            where.Add($"""
                       EXISTS(
                           SELECT 1 
                           FROM {itemType.GetItemPropertySqlTable()} 
                           WHERE {itemType.GetItemSqlTable()}.{nameof(IItem.Id)} = {itemType.GetItemPropertySqlTable()}.{TableFieldName.ItemProperty.ItemId}
                               AND {condition.Condition}
                       )
                       """);

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

        //---
        var sql = $"""
                   BEGIN TRANSACTION;

                   DECLARE @deletedIds TABLE ( id VARCHAR(256) );

                   DELETE FROM {itemType.GetItemSqlTable()}
                   OUTPUT deleted.id INTO @deletedIds
                   WHERE {string.Join(" AND ", where)};

                   DELETE FROM {itemType.GetItemPropertySqlTable()}
                   WHERE {itemType.GetItemPropertySqlTable()}.{TableFieldName.ItemProperty.ItemId} IN (SELECT id FROM @deletedIds);

                   COMMIT TRANSACTION;

                   SELECT COUNT(*) AS Counter FROM @deletedIds;
                   """;

        var deletedRows = await connectionManager.ExecuteScalarAsync<int>(sql, parameters);

        return this.SuccessfulResponse(new ItemDeleteResponse {DeletedRows = deletedRows});
    }
}