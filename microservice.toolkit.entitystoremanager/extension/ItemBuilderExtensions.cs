using microservice.toolkit.entitystoremanager.entity;

using System;
using System.Linq;

namespace microservice.toolkit.entitystoremanager.extension;

internal static class ItemBuilder
{
    internal static TSource Build<TSource>(string id, bool? enabled, long? inserted, long? updated, string updater)
        where TSource : IItem, new()
    {
        var objectType = typeof(TSource);
        var instance = (TSource)Activator.CreateInstance(objectType);

        if (instance == null)
        {
            return default;
        }

        instance.Id = id;
        instance.Enabled = enabled;
        instance.Inserted = inserted;
        instance.Updated = updated;
        instance.Updater = updater;

        return instance;
    }
}

internal static class ItemBuilderExtensions
{
    internal static void Build<TSource>(this TSource source, string propertyName, object value, int order)
        where TSource : IItem, new()
    {
        var objectType = typeof(TSource);
        var properties = objectType.GetItemProperties();
        var property = properties.FirstOrDefault(p => p.Name == propertyName);

        if (property == default)
        {
            return;
        }

        switch (value)
        {
            case not null when property.PropertyType is {IsArray: true, IsEnum: false}:
                var elementType = property.PropertyType.GetElementType();
                if (elementType != null)
                {
                    var arr = property.GetValue(source) as Array ??
                              Array.CreateInstance(elementType, 0);
                    var newArraySize = Math.Max(arr.Length, order + 1);
                    var newArray = Array.CreateInstance(elementType, newArraySize);

                    Array.Copy(arr, newArray, arr.Length);

                    newArray.SetValue(value, order);
                    property.SetValue(source, newArray);
                }

                break;
            case int intValue when property.PropertyType is {IsEnum: true, IsArray: false}:
                var enumValue = Enum.ToObject(property.PropertyType, intValue);
                property.SetValue(source, enumValue);
                break;
            case long longValue:
                property.SetValue(source, longValue);
                break;
            case float floatValue:
                property.SetValue(source, floatValue);
                break;
            case int intValue when property.PropertyType is {IsEnum: false}:
                property.SetValue(source, intValue);
                break;
            case bool boolValue:
                property.SetValue(source, boolValue);
                break;
            case string stringValue:
                property.SetValue(source, stringValue);
                break;
        }
    }
}