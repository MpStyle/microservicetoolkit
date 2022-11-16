using System;

namespace microservice.toolkit.messagemediator.attribute;

/// <summary>
/// Specifies the pattern for a <see cref="Service{TRequest, TPayload}"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MicroService : Attribute
{
    public string Pattern { get; }

    public MicroService(string pattern = null)
    {
        this.Pattern = pattern;
    }
}