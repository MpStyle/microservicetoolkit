#nullable enable
using System;
using System.Linq;
using System.Reflection;

namespace microservice.toolkit.connection.extensions.objectmapper;

internal class TypeMap
{
    public TypeProperty[] TypeTypeProperties { get; }

    public TypeMap(IReflect t)
    {
        // Select only public and "of instance" properties
        this.TypeTypeProperties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(pi=>new TypeProperty(pi)).ToArray();
    }

    public object? this[object target, string name]
    {
        get
        {
            var prop = this.TypeTypeProperties.FirstOrDefault(pi => pi.Name.Equals(name));

            if (prop == null)
            {
                throw new ArgumentOutOfRangeException(nameof(name));
            }

            if (prop.IsReadable == false)
            {
                throw new Exception($"{name} is not readable");
            }

            return prop.GetValue(target);
        }
        set
        {
            var prop = this.TypeTypeProperties.FirstOrDefault(pi => pi.Name.Equals(name));

            if (prop == null)
            {
                throw new ArgumentOutOfRangeException(nameof(name));
            }
            
            if (prop.IsWritable == false)
            {
                throw new Exception($"{name} is not writable");
            }

            prop.SetValue(target, value);
        }
    }
}