#nullable enable
using System;
using System.Reflection;

namespace microservice.toolkit.connection.extensions.objectmapper;

internal class TypeProperty(PropertyInfo propertyInfo)
{
    public string Name { get; } = propertyInfo.Name;
    public bool IsReadable { get;  } = propertyInfo.CanRead;
    public bool IsWritable { get;  } = propertyInfo.CanWrite;
    public Type Type { get; } = propertyInfo.PropertyType;

    public object? GetValue(object target)
    {
        return propertyInfo.GetValue(target);
    }
    
    public void SetValue(object target, object? value)
    {
        propertyInfo.SetValue(target, value);
    }
}