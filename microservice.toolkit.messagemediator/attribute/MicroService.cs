using System;

namespace microservice.toolkit.messagemediator.attribute;

[AttributeUsage(AttributeTargets.Class)]
public class MicroService : Attribute
{
    public string Name { get; }

    public MicroService(string name)
    {
        this.Name = name;
    }
}