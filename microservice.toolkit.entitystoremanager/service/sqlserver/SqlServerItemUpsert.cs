using microservice.toolkit.connectionmanager;
using microservice.toolkit.core.entity;
using microservice.toolkit.entitystoremanager.book;
using microservice.toolkit.entitystoremanager.entity;
using microservice.toolkit.entitystoremanager.entity.service;
using microservice.toolkit.entitystoremanager.extension;
using microservice.toolkit.messagemediator;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;

namespace microservice.toolkit.entitystoremanager.service.sqlserver;

public class SqlServerItemUpsert<TSource> : Service<ItemUpsertRequest<TSource>, ItemUpsertResponse>
    where TSource : IItem, new()
{
    private readonly DbConnection dbConnection;

    public SqlServerItemUpsert(DbConnection dbConnection)
    {
        this.dbConnection = dbConnection;
    }

    public override async Task<ServiceResponse<ItemUpsertResponse>> Run(ItemUpsertRequest<TSource> request)
    {
        if (request.Item?.Updater == null)
        {
            return this.UnsuccessfulResponse(EntityError.ItemUpsertInvalidRequest);
        }

        if (this.dbConnection is not SqlConnection sqlServerConnection)
        {
            return this.UnsuccessfulResponse(EntityError.ItemUpsertInvalidDatabaseConnection);
        }

        var itemType = typeof(TSource);
        var itemId = request.Item.Id ?? Guid.NewGuid().ToString();
        var itemPropertyNames = itemType.GetItemPropertyNames();

        // Upserts item
        await this.dbConnection.ExecuteNonQueryAsync(
            $"""
                 MERGE INTO {itemType.GetItemSqlTable()} AS target
                 USING (SELECT @Id AS {nameof(IItem.Id)}, @Type AS {TableFieldName.Item.Type}) AS source
                 ON (target.{nameof(IItem.Id)} = source.{nameof(IItem.Id)} AND target.{TableFieldName.Item.Type} = source.{TableFieldName.Item.Type})
                 WHEN MATCHED THEN
                     UPDATE SET {nameof(IItem.Inserted)} = @Inserted,
                                 {nameof(IItem.Updated)} = @Updated,
                                 {nameof(IItem.Updater)} = @Updater,
                                 {nameof(IItem.Enabled)} = @Enabled
                 WHEN NOT MATCHED THEN
                     INSERT ({nameof(IItem.Id)}, {TableFieldName.Item.Type}, {nameof(IItem.Inserted)}, {nameof(IItem.Updated)}, {nameof(IItem.Updater)}, {nameof(IItem.Enabled)})
                     VALUES (@Id, @Type, @Inserted, @Updated, @Updater, @Enabled);
             """,
            new Dictionary<string, object>
            {
                { "@Id", itemId },
                { "@Type", itemType.GetItemName() },
                { "@Inserted", request.Item.Inserted ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "@Updated", request.Item.Updated ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "@Updater", request.Item.Updater },
                { "@Enabled", request.Item.Enabled ?? true },
            });

        // Upserts item properties
        var dataTable = this.CreateUpdateRecordsSqlParam(request.Item);
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var tempTableName = $"#TempTable_{Guid.NewGuid().ToString().Replace("-", string.Empty)}";
            await this.dbConnection.ExecuteNonQueryAsync(
                $"SELECT * INTO {tempTableName} FROM {itemType.GetItemPropertySqlTable()} WHERE 1 = 0;");
            using (var bulkCopy = new SqlBulkCopy(sqlServerConnection))
            {
                bulkCopy.DestinationTableName = tempTableName;
                await bulkCopy.WriteToServerAsync(dataTable);
            }

            var mergeSql = $"""
                MERGE INTO [{itemType.GetItemPropertySqlTable()}] AS Target
                USING {tempTableName} AS Source
                ON Target.{TableFieldName.ItemProperty.ItemId} = Source.{TableFieldName.ItemProperty.ItemId} 
                    AND Target.[{TableFieldName.ItemProperty.Key}] = Source.[{TableFieldName.ItemProperty.Key}] 
                    AND Target.[{TableFieldName.ItemProperty.Order}] = Source.[{TableFieldName.ItemProperty.Order}]
                WHEN MATCHED THEN
                    UPDATE SET Target.{TableFieldName.ItemProperty.StringValue} = Source.{TableFieldName.ItemProperty.StringValue},
                        Target.{TableFieldName.ItemProperty.IntValue} = Source.{TableFieldName.ItemProperty.IntValue},
                        Target.{TableFieldName.ItemProperty.LongValue} = Source.{TableFieldName.ItemProperty.LongValue},
                        Target.{TableFieldName.ItemProperty.FloatValue} = Source.{TableFieldName.ItemProperty.FloatValue},
                        Target.{TableFieldName.ItemProperty.BoolValue} = Source.{TableFieldName.ItemProperty.BoolValue}
                WHEN NOT MATCHED THEN
                    INSERT ({TableFieldName.ItemProperty.ItemId},
                            [{TableFieldName.ItemProperty.Key}],
                            {TableFieldName.ItemProperty.StringValue},
                            {TableFieldName.ItemProperty.IntValue},
                            {TableFieldName.ItemProperty.LongValue},
                            {TableFieldName.ItemProperty.FloatValue},
                            {TableFieldName.ItemProperty.BoolValue},
                            [{TableFieldName.ItemProperty.Order}])
                    VALUES (Source.{TableFieldName.ItemProperty.ItemId},
                            Source.[{TableFieldName.ItemProperty.Key}],
                            Source.{TableFieldName.ItemProperty.StringValue},
                            Source.{TableFieldName.ItemProperty.IntValue},
                            Source.{TableFieldName.ItemProperty.LongValue},
                            Source.{TableFieldName.ItemProperty.FloatValue},
                            Source.{TableFieldName.ItemProperty.BoolValue},
                            Source.[{TableFieldName.ItemProperty.Order}]);
            """;
            await this.dbConnection.ExecuteNonQueryAsync(mergeSql);
            await this.dbConnection.ExecuteNonQueryAsync($"DROP TABLE {tempTableName};");

            scope.Complete();
        }

        // Deletes unused properties
        var deleteWhere = new List<string>
        {
            $"{itemType.GetItemPropertySqlTable()}.{TableFieldName.ItemProperty.ItemId} = @ItemId"
        };
        var deleteParameters = new Dictionary<string, object> { { "@ItemId", itemId } };
        var deleteParameterNames = new List<string>();

        for (var i = 0; i < itemPropertyNames.Length; i++)
        {
            var parameterName = $"@{nameof(itemPropertyNames)}_{i}";
            deleteParameterNames.Add(parameterName);
            deleteParameters.Add(parameterName, itemPropertyNames[i]);
        }

        deleteWhere.Add(
            $"{itemType.GetItemPropertySqlTable()}.[{TableFieldName.ItemProperty.Key}] NOT IN ({string.Join(",", deleteParameterNames)})");

        var deleteSql = $"DELETE FROM {itemType.GetItemPropertySqlTable()} WHERE {string.Join(" AND ", deleteWhere)}";

        await this.dbConnection.ExecuteNonQueryAsync(deleteSql, deleteParameters);

        if (request.ReturnEmptyResponse)
        {
            return this.SuccessfulResponse(new ItemUpsertResponse());
        }

        return this.SuccessfulResponse(new ItemUpsertResponse { ItemId = itemId });
    }

    private void AddToDataTable(object value, PropertyInfo propertyInfo, string itemId, int order, ref DataTable table)
    {
        var propertyName = typeof(TSource).GetItemPropertyName(propertyInfo); // propertyInfo.Name;

        switch (value)
        {
            case not null when propertyInfo.PropertyType.IsEnum:
                table.Rows.Add(
                    itemId,
                    propertyName,
                    null,
                    (int)value,
                    null,
                    null,
                    null,
                    order);
                break;
            case Array array:
                var i = 0;
                foreach (var arrayItem in array)
                {
                    this.AddToDataTable(arrayItem, propertyInfo, itemId, i++, ref table);
                }

                break;
            case long:
            case float:
            case bool:
            case int:
            case string:
                table.Rows.Add(
                    itemId,
                    propertyName,
                    value as string,
                    value as int?,
                    value as long?,
                    value as float?,
                    value as bool?,
                    order);
                break;
        }
    }

    private DataTable CreateUpdateRecordsSqlParam(TSource item)
    {
        var itemType = typeof(TSource);
        var itemProperties = itemType.GetItemProperties();
        var table = new DataTable();

        table.Columns.Add(TableFieldName.ItemProperty.ItemId, typeof(string));
        table.Columns.Add(TableFieldName.ItemProperty.Key, typeof(string));
        table.Columns.Add(TableFieldName.ItemProperty.StringValue, typeof(string));
        table.Columns.Add(TableFieldName.ItemProperty.IntValue, typeof(int));
        table.Columns.Add(TableFieldName.ItemProperty.LongValue, typeof(long));
        table.Columns.Add(TableFieldName.ItemProperty.FloatValue, typeof(float));
        table.Columns.Add(TableFieldName.ItemProperty.BoolValue, typeof(bool));
        table.Columns.Add(TableFieldName.ItemProperty.Order, typeof(int));

        foreach (var itemProperty in itemProperties)
        {
            var value = itemProperty.GetValue(item);
            this.AddToDataTable(value, itemProperty, item.Id, 0, ref table);
        }

        return table;
    }
}