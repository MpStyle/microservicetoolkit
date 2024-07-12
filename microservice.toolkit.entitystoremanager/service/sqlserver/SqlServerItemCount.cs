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

public class SqlServerItemCount<TSource> : Service<ItemCountRequest, ItemCountResponse>
    where TSource : IItem, new()
{
    private readonly DbConnection connectionManager;

    public SqlServerItemCount(DbConnection connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    public override async Task<ServiceResponse<ItemCountResponse>> Run(ItemCountRequest request)
    {
        var where = new List<string>();
        var parameters = new Dictionary<string, object>();

        if (request.IncludeDisabled == false)
        {
            where.Add($"{nameof(Item)}.{Item.Enabled} = 1");
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

        if (request.Id.IsNullOrEmpty() == false)
        {
            where.Add($"id = @Id");
            parameters.Add("@Id", request.Id);
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
        parameters.Add("@Type", typeof(TSource).GetItemName());

        var itemsSql =
            $"SELECT COUNT({Item.Id}) AS Counter FROM {nameof(Item)} WHERE {string.Join(" AND ", where)}";

        var itemCount =
            await this.connectionManager.ExecuteFirstAsync(itemsSql, reader => reader.GetInt32(0), parameters);

        return this.SuccessfulResponse(new ItemCountResponse
        {
            Counter = itemCount
        });
    }
}