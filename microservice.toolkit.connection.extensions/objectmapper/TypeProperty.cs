#nullable enable
using System;
using System.Reflection;

namespace microservice.toolkit.connection.extensions.objectmapper;

internal class TypeProperty
{
    private readonly PropertyInfo propertyInfo;

    public string Name { get; }
    public bool IsReadable { get;  }
    public bool IsWritable { get;  }
    public Type Type { get; }

    public TypeProperty(PropertyInfo propertyInfo)
    {
        this.propertyInfo = propertyInfo;
        this.Name = propertyInfo.Name;
        this.IsReadable = propertyInfo.CanRead;
        this.IsWritable = propertyInfo.CanWrite;
        this.Type = propertyInfo.PropertyType;
    }

    public object? GetValue(object target)
    {
        return this.propertyInfo.GetValue(target);
    }
    
    public void SetValue(object target, object? value)
    {
        this.propertyInfo.SetValue(target, value);
    }
}