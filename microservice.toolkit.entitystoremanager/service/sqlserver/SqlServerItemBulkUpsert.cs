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
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace microservice.toolkit.entitystoremanager.service.sqlserver;

public class SqlServerItemBulkUpsert<TSource> : Service<ItemBulkUpsertRequest<TSource>, ItemBulkUpsertResponse>
    where TSource : IItem, new()
{
    private readonly DbConnection dbConnection;

    public SqlServerItemBulkUpsert(DbConnection dbConnection)
    {
        this.dbConnection = dbConnection;
    }

    public override async Task<ServiceResponse<ItemBulkUpsertResponse>> Run(ItemBulkUpsertRequest<TSource> request)
    {
        if (request.Items.IsNullOrEmpty() || request.Items.Any(i => i.Updater == null))
        {
            return this.UnsuccessfulResponse(EntityError.ItemUpsertInvalidRequest);
        }

        if (this.dbConnection is not SqlConnection sqlServerConnection)
        {
            return this.UnsuccessfulResponse(EntityError.ItemUpsertInvalidDatabaseConnection);
        }

        await sqlServerConnection.SafeOpenAsync();
        var itemType = typeof(TSource);
        var itemPropertyNames = itemType.GetItemPropertyNames();

        // Upserts item
        var itemDataTable = this.CreateItemsDataTable(request.Items);
        if (itemDataTable.Rows.Count > 0)
        {
            using var command = sqlServerConnection.CreateCommand();
            command.CommandText = "ItemBulkUpsert";
            command.CommandType = CommandType.StoredProcedure;

            var param = command.CreateParameter();

            param.SqlDbType = SqlDbType.Structured;
            param.TypeName = "ItemType";
            param.ParameterName = "@UpdateRecords";
            param.Value = itemDataTable;
            param.SqlDbType = SqlDbType.Structured;

            command.Parameters.Add(param);

            await command.ExecuteNonQueryAsync();
        }

        // Upserts item properties
        var dataTable = this.CreateItemPropertiesDataTable(request.Items);
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
        var deletedItemId = itemDataTable.Rows.OfType<DataRow>().Select(dr => dr.ItemArray[0] as string).ToArray();
        var deleteWhere = new List<string>();
        var deleteParameters = new Dictionary<string, object>();

        var deleteItemParameterNames = new List<string>();
        for (var i = 0; i < deletedItemId.Length; i++)
        {
            var parameterName = $"@{nameof(deletedItemId)}_{i}";
            deleteItemParameterNames.Add(parameterName);
            deleteParameters.Add(parameterName, deletedItemId[i]);
        }

        deleteWhere.Add(
            $"{nameof(ItemProperty)}.[{ItemProperty.ItemId}] IN ({string.Join(",", deleteItemParameterNames)})");

        var deleteParameterNames = new List<string>();

        for (var i = 0; i < itemPropertyNames.Length; i++)
        {
            var parameterName = $"@{nameof(itemPropertyNames)}_{i}";
            deleteParameterNames.Add(parameterName);
            deleteParameters.Add(parameterName, itemPropertyNames[i]);
        }

        deleteWhere.Add(
            $"{nameof(ItemProperty)}.[{ItemProperty.Key}] NOT IN ({string.Join(",", deleteParameterNames)})");

        var deleteSql = $"DELETE FROM {nameof(ItemProperty)} WHERE {string.Join(" AND ", deleteWhere)}";

        await this.dbConnection.ExecuteNonQueryAsync(deleteSql, deleteParameters);

        if (request.ReturnEmptyResponse)
        {
            return this.SuccessfulResponse(new ItemBulkUpsertResponse());
        }

        return this.SuccessfulResponse(new ItemBulkUpsertResponse {ItemIds = deletedItemId});
    }

    private void AddToDataTable(object value, PropertyInfo propertyInfo, string itemId, int order, ref DataTable table)
    {
        var propertyName = typeof(TSource).GetItemPropertyName(propertyInfo);

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

    private DataTable CreateItemPropertiesDataTable(TSource[] items)
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
            foreach (var item in items)
            {
                var value = itemProperty.GetValue(item);
                this.AddToDataTable(value, itemProperty, item.Id, 0, ref table);
            }
        }

        return table;
    }

    private DataTable CreateItemsDataTable(TSource[] items)
    {
        var table = new DataTable();

        table.Columns.Add(Item.Id, typeof(string));
        table.Columns.Add(Item.Type, typeof(string));
        table.Columns.Add(Item.Inserted, typeof(long));
        table.Columns.Add(Item.Updated, typeof(long));
        table.Columns.Add(Item.Updater, typeof(string));
        table.Columns.Add(Item.Enabled, typeof(bool));

        foreach (var item in items)
        {
            table.Rows.Add(
                item.Id ?? Guid.NewGuid().ToString(),
                typeof(TSource).GetItemName(),
                item.Inserted,
                item.Updated,
                item.Updater,
                item.Enabled
            );
        }

        return table;
    }
}