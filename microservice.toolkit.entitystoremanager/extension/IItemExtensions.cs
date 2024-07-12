using microservice.toolkit.entitystoremanager.attribute;
using microservice.toolkit.entitystoremanager.book;
using microservice.toolkit.entitystoremanager.entity;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace microservice.toolkit.entitystoremanager.extension;

internal static class TypeExtensions
{
    private static readonly string[] ExcludedPropertyNames =
    [
        Item.Inserted, Item.Updated, Item.Updater, Item.Enabled, Item.Id
    ];

    private static readonly ConcurrentDictionary<Type, CacheItem> Cache = new();

    private static CacheItem AddToCache(this Type itemType)
    {
        if (itemType.GetInterfaces().Contains(typeof(IItem)) == false)
        {
            throw new Exception($"Type {itemType.FullName} does not implement {nameof(IItem)}");
        }

        var pis = itemType.GetCustomAttribute<ItemAttribute>();

        // get all properties in itemType
        var propertyInfos = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(propertyInfo =>
                propertyInfo.Name != nameof(IItem.Id) &&
                propertyInfo.Name != nameof(IItem.Enabled) &&
                propertyInfo.Name != nameof(IItem.Inserted) &&
                propertyInfo.Name != nameof(IItem.Updated) &&
                propertyInfo.Name != nameof(IItem.Updater)
            )
            .Where(propertyInfo => propertyInfo.GetCustomAttribute<ItemPropertyIgnoreAttribute>() == null)
            .ToDictionary(p =>
            {
                var attribute = p.GetCustomAttribute<ItemPropertyAttribute>();
                return attribute != null ? attribute.Name : p.Name;
            });

        var cacheItem = new CacheItem
        {
            Name = pis?.Name ?? itemType.FullName,
            Properties = new ConcurrentDictionary<string, PropertyInfo>(propertyInfos)
        };

        Cache.TryAdd(itemType, cacheItem);

        return cacheItem;
    }

    public static string[] GetItemPropertyNames(this Type itemType)
    {
        if (!Cache.TryGetValue(itemType, out var value))
        {
            value = itemType.AddToCache();
        }

        return value.Properties.Keys.ToArray();
    }

    public static PropertyInfo[] GetItemProperties(this Type itemType)
    {
        if (!Cache.TryGetValue(itemType, out var value))
        {
            value = itemType.AddToCache();
        }

        return value.Properties.Values.ToArray();
    }

    public static string GetItemName(this Type itemType)
    {
        if (!Cache.TryGetValue(itemType, out var value))
        {
            value = itemType.AddToCache();
        }

        return value.Name;
    }

    public static string GetItemPropertyName(this Type itemType, PropertyInfo property)
    {
        if (!Cache.TryGetValue(itemType, out var value))
        {
            value = itemType.AddToCache();
        }

        return value.Properties.FirstOrDefault(p => p.Value == property).Key;
    }
    
    public static PropertyInfo GetItemProperty(this Type itemType, string propertyName)
    {
        if (!Cache.TryGetValue(itemType, out var value))
        {
            value = itemType.AddToCache();
        }
    
        return value.Properties.TryGetValue(propertyName, out var property) ? property : default;
    }
}

internal class CacheItem
{
    public string Name { get; set; }
    public ConcurrentDictionary<string, PropertyInfo> Properties { get; set; }
}