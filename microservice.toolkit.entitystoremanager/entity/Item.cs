using microservice.toolkit.entitystoremanager.extension;

using System;
using System.Linq.Expressions;

namespace microservice.toolkit.entitystoremanager.entity;

public class Item
{
    public static string GetPropertyName<T>(Expression<Func<T, object>> propertyExpression) where T : IItem
    {
        var propertyName = propertyExpression.Body switch
        {
            MemberExpression memberExpression => memberExpression.Member.Name,
            UnaryExpression { Operand: MemberExpression operand } => operand.Member.Name,
            _ => throw new ArgumentException("Invalid property expression", nameof(propertyExpression))
        };

        return typeof(T).GetItemPropertyName(propertyName);
    }
}