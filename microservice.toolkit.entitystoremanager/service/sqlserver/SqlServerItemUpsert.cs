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
        await this.dbConnection.ExecuteStoredProcedureAsync("ItemUpsert", new Dictionary<string, object>
        {
            {"@Id", itemId},
            {"@Type", itemType.GetItemName()},
            {"@Inserted", request.Item.Inserted ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds()},
            {"@Updated", request.Item.Updated ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds()},
            {"@Updater", request.Item.Updater},
            {"@Enabled", request.Item.Enabled ?? true},
        });

        // Upserts item properties
        var dataTable = this.CreateUpdateRecordsSqlParam(request.Item);
        if (dataTable.Rows.Count > 0)
        {
            using var command = sqlServerConnection.CreateCommand();
            command.CommandText = "ItemPropertyBulkUpsert";
            command.CommandType = CommandType.StoredProcedure;

            var param = command.CreateParameter();

            param.SqlDbType = SqlDbType.Structured;
            param.TypeName = "ItemPropertyType";
            param.ParameterName = "@UpdateRecords";
            param.Value = dataTable;
            param.SqlDbType = SqlDbType.Structured;

            command.Parameters.Add(param);

            await command.ExecuteNonQueryAsync();
        }

        // Deletes unused properties
        var deleteWhere = new List<string> { $"{nameof(ItemProperty)}.{ItemProperty.ItemId} = @ItemId" };
        var deleteParameters = new Dictionary<string, object> { { "@ItemId", itemId } };
        var deleteParameterNames = new List<string>();

        for (var i = 0; i < itemPropertyNames.Length; i++)
        {
            var parameterName = $"@{nameof(itemPropertyNames)}_{i}";
            deleteParameterNames.Add(parameterName);
            deleteParameters.Add(parameterName, itemPropertyNames[i]);
        }

        deleteWhere.Add($"{nameof(ItemProperty)}.[{ItemProperty.Key}] NOT IN ({string.Join(",", deleteParameterNames)})");

        var deleteSql = $"DELETE FROM {nameof(ItemProperty)} WHERE {string.Join(" AND ", deleteWhere)}";

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

        table.Columns.Add(ItemProperty.ItemId, typeof(string));
        table.Columns.Add(ItemProperty.Key, typeof(string));
        table.Columns.Add(ItemProperty.StringValue, typeof(string));
        table.Columns.Add(ItemProperty.IntValue, typeof(int));
        table.Columns.Add(ItemProperty.LongValue, typeof(long));
        table.Columns.Add(ItemProperty.FloatValue, typeof(float));
        table.Columns.Add(ItemProperty.BoolValue, typeof(bool));
        table.Columns.Add(ItemProperty.Order, typeof(int));

        foreach (var itemProperty in itemProperties)
        {
            var value = itemProperty.GetValue(item);
            this.AddToDataTable(value, itemProperty, item.Id, 0, ref table);
        }

        return table;
    }
}