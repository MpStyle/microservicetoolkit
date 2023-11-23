using System;
using System.Collections.Generic;
using System.Data.Common;

namespace microservice.toolkit.orm;

public class Mito
{
    private readonly MitoCache cache = new();

    public Func<DbDataReader, T> MapperFunc<T>()
    {
        return reader =>
        {
            var t = typeof(T);

            // Type is not in the cache and it can not be in the cache
            if (this.cache.Exists(t) == false && this.cache.Add(t) == false)
            {
                return default;
            }

            var instance = Activator.CreateInstance(t);
            
            // Arrays of the instance, managed using lists (to easy add new items) and converted to array and the and of mapper function
            var arrays = new Dictionary<string, List<object>>();

            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.IsDBNull(i))
                {
                    continue;
                }

                var columnName = reader.GetName(i);

                // Property is not a column
                if (this.cache.TryGet(t, columnName, out var propertyInfo) == false || propertyInfo==null)
                {
                    continue;
                }

                var value = reader.GetValue(i);

                if (propertyInfo.PropertyType.IsArray)
                {
                    if (arrays.ContainsKey(columnName) == false)
                    {
                        arrays.Add(columnName, new List<object>());
                    }

                    arrays[columnName].Add(value);
                }
                else
                {
                    propertyInfo.SetValue(instance, value);
                }
            }

            // Converts list in array of instance
            foreach (var array in arrays)
            {
                this.cache.Get(t, array.Key).SetValue(instance, array.Value);
            }

            return (T)instance;
        };
    }
}