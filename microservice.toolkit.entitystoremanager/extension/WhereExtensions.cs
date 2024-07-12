using System;
using System.Collections.Generic;
using System.Linq;

using microservice.toolkit.core.extension;
using microservice.toolkit.entitystoremanager.book;
using microservice.toolkit.entitystoremanager.entity;

namespace microservice.toolkit.entitystoremanager.extension;

internal static class WhereExtensions
{
    private static string GenerateUniqueParamName()
    {
        return $"@param_{Guid.NewGuid().ToString()}".Replace("-", "_");
    }

    private static string FieldName(object value)
    {
        return value switch
        {
            string => TableFieldName.ItemProperty.StringValue,
            bool => TableFieldName.ItemProperty.BoolValue,
            float => TableFieldName.ItemProperty.FloatValue,
            int => TableFieldName.ItemProperty.IntValue,
            long => TableFieldName.ItemProperty.LongValue,
            _ => string.Empty
        };
    }

    internal static DbFilter ToSqlServerCondition<TSource>(this IWhere where, string tableName)
    {
        var itemType = typeof(TSource);
        var condition = string.Empty;
        var parameters = new Dictionary<string, object>();
        var placeholderKey = GenerateUniqueParamName();

        switch (where)
        {
            case IsNullWhere isw:
                condition = $@"(
                    (
                        {tableName}.[{TableFieldName.ItemProperty.Key}] = {placeholderKey} 
                        AND {tableName}.{TableFieldName.ItemProperty.StringValue} IS NULL
                        AND {tableName}.{TableFieldName.ItemProperty.BoolValue} IS NULL
                        AND {tableName}.{TableFieldName.ItemProperty.FloatValue} IS NULL
                        AND {tableName}.{TableFieldName.ItemProperty.IntValue} IS NULL
                        AND {tableName}.{TableFieldName.ItemProperty.LongValue} IS NULL
                    )
                    OR 
                    (
                        NOT EXISTS (
                            SELECT 1 
                            FROM {tableName} 
                            WHERE {itemType.GetItemSqlTable()}.{nameof(IItem.Id)} = {tableName}.{TableFieldName.ItemProperty.ItemId}
                                AND {tableName}.[{TableFieldName.ItemProperty.Key}] = {placeholderKey} 
                        ) 
                    )
                )";
                parameters.Add(placeholderKey, isw.Key);
                break;
            case Where w:
                var placeholderNameValue = GenerateUniqueParamName();
                var whereFieldName = FieldName(w.Value);

                condition = w.Operator switch
                {
                    Operator.Like =>
                        $"{tableName}.{whereFieldName} {w.Operator.ToSqlServerOperatorString()} CONCAT('%', {placeholderNameValue}, '%') AND {tableName}.[{TableFieldName.ItemProperty.Key}] = {placeholderKey}",
                    Operator.StartingLike =>
                        $"{tableName}.{whereFieldName} {w.Operator.ToSqlServerOperatorString()} CONCAT({placeholderNameValue}, '%') AND {tableName}.[{TableFieldName.ItemProperty.Key}] = {placeholderKey}",
                    Operator.EndingLike =>
                        $"{tableName}.{whereFieldName} {w.Operator.ToSqlServerOperatorString()} CONCAT('%', {placeholderNameValue}) AND {tableName}.[{TableFieldName.ItemProperty.Key}] = {placeholderKey}",
                    _ =>
                        $"{tableName}.{whereFieldName} {w.Operator.ToSqlServerOperatorString()} {placeholderNameValue} AND {tableName}.[{TableFieldName.ItemProperty.Key}] = {placeholderKey}"
                };

                parameters.Add(placeholderNameValue, w.Value);

                parameters.Add(placeholderKey, w.Key);

                break;
            case LogicWhere ow:

                var innerConditions = ow.Conditions.Select(c =>
                {
                    var innerCondition = c.ToSqlServerCondition<TSource>(tableName);
                    foreach (var param in innerCondition.Parameters)
                    {
                        parameters.Add(param.Key, param.Value);
                    }

                    return
                        $"EXISTS (SELECT 1 FROM {tableName} WHERE {itemType.GetItemSqlTable()}.{nameof(IItem.Id)} = {tableName}.{TableFieldName.ItemProperty.ItemId} AND {innerCondition.Condition})";
                });
                condition = $"({string.Join($" {ow.Op.ToSqlServerOperatorString()} ", innerConditions)})";

                break;
            case InWhere inw when inw.Values.IsNullOrEmpty() == false:
                var parameterNames = new List<string>();
                foreach (var value in inw.Values)
                {
                    var parameterName = GenerateUniqueParamName();

                    parameterNames.Add(parameterName);
                    parameters.Add(parameterName, value);
                }

                var inWhereFieldName = FieldName(inw.Values.First());

                condition =
                    $"{tableName}.{TableFieldName.ItemProperty.Key} = {placeholderKey} AND {tableName}.{inWhereFieldName} IN ({string.Join(",", parameterNames)})";

                break;
        }

        return new DbFilter { Condition = condition, Parameters = parameters };
    }
}