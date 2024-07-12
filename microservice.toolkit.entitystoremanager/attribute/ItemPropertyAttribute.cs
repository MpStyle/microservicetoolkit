using System;

namespace microservice.toolkit.entitystoremanager.attribute;

[AttributeUsage(AttributeTargets.Property)]
public class ItemPropertyAttribute : Attribute
{
    public string Name { get; }

    public ItemPropertyAttribute(string name)
    {
        this.Name = name;
    }
}