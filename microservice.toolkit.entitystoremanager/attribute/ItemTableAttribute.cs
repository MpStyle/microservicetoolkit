using System;

namespace microservice.toolkit.entitystoremanager.attribute;

[AttributeUsage(AttributeTargets.Class)]
public class ItemTableAttribute : Attribute
{
    public string Prefix { get; }
    public string ItemTable { get; }
    public string ItemPropertyTable { get; }
    
    public ItemTableAttribute(string prefix)
    {
        this.Prefix = prefix;
    }
    
    public ItemTableAttribute(string itemTable, string itemPropertyTable)
    {
        this.ItemTable = itemTable;
        this.ItemPropertyTable = itemPropertyTable;
    }
}