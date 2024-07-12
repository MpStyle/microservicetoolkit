using System;

namespace microservice.toolkit.entitystoremanager.attribute;

[AttributeUsage(AttributeTargets.Class)]
public class ItemAttribute : Attribute
{
    public string Name { get; }

    public ItemAttribute(string name)
    {
        this.Name = name;
    }
}