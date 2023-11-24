using System;

namespace microservice.toolkit.orm;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MitoColumnAttribute: Attribute
{
    public string Name { get; set; }

    public MitoColumnAttribute(string name)
    {
        Name = name;
    }
}