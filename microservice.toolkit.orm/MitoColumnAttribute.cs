using System;

namespace microservice.toolkit.orm;

[AttributeUsage(AttributeTargets.Property)]
public class MitoColumnAttribute: Attribute
{
    public string Name { get; set; }

    public MitoColumnAttribute(string name)
    {
        Name = name;
    }
}