using System;

namespace microservice.toolkit.entitystoremanager.attribute;

[AttributeUsage(AttributeTargets.Class)]
public class ItemTableAttribute : Attribute
{
    public string Prefix { get; }
    
    public ItemTableAttribute(string prefix)
    {
        this.Prefix = prefix;
    }
}