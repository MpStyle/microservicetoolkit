using microservice.toolkit.entitystoremanager.book;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace microservice.toolkit.entitystoremanager.extension;

internal static class TypeExtensions
{
    private static readonly string[] ExcludedPropertyNames =
    {
        Item.Inserted, Item.Updated, Item.Updater, Item.Enabled, Item.Id
    };

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Cache = new();

    public static PropertyInfo[] GetItemProperties(this Type itemType)
    {
        if (Cache.ContainsKey(itemType))
        {
            return Cache[itemType];
        }

        var pis = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => ExcludedPropertyNames.Contains(p.Name) == false)
            .ToArray();

        Cache.TryAdd(itemType, pis);

        return pis;
    }
}