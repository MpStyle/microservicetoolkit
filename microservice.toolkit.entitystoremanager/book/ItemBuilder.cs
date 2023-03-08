using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using microservice.toolkit.entitystoremanager.entity;

namespace microservice.toolkit.entitystoremanager.book;

internal class ItemBuilder
{
    private static readonly Dictionary<Type, PropertyInfo[]> PropertiesInfoCache = new();

    internal TSource Build<TSource>(string id, bool? enabled, long? inserted, long? updated, string updater)
        where TSource : IItem, new()
    {
        var objectType = typeof(TSource);
        var instance = (TSource) Activator.CreateInstance(objectType);

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

    internal void Build<TSource>(string propertyName, object value, int order, ref TSource source)
        where TSource : IItem, new()
    {
        var objectType = typeof(TSource);

        if (PropertiesInfoCache.ContainsKey(objectType) == false)
        {
            PropertiesInfoCache[objectType] = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        var properties = PropertiesInfoCache[objectType];

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