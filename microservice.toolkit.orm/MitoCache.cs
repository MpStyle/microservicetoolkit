using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace microservice.toolkit.orm;

internal class MitoCache
{
    // Type full name => attribute name => T property info
    private readonly Dictionary<Type, Dictionary<string, PropertyInfo>> cache = new();

    public bool Add(Type t)
    {
        var typeFullName = t.FullName;

        if (typeFullName == null)
        {
            return false;
        }

        this.cache.Add(t, new Dictionary<string, PropertyInfo>());

        var typeProperties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in typeProperties)
        {
            var attrs = prop.GetCustomAttributes(true);

            foreach (var attr in attrs.OfType<MitoColumnAttribute>())
            {
                var columnName = attr.Name;

                this.cache[t].Add(columnName, prop);
            }
        }

        return true;
    }

    public bool Exists(Type t)
    {
        return this.cache.ContainsKey(t);
    }

    public bool Exists(Type t, string columnName)
    {
        return this.cache.ContainsKey(t) && this.cache[t].ContainsKey(columnName);
    }
    
    public PropertyInfo Get(Type t, string columnName)
    {
        return this.cache[t][columnName];
    }

    public bool TryGet(Type t, string columnName, out PropertyInfo propertyInfo)
    {
        var result = this.Exists(t, columnName);

        propertyInfo = result ? this.cache[t][columnName] : null;

        return result;
    }
}