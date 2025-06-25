using System;

namespace microservice.toolkit.messagemediator.attribute;

/// <summary>
/// Specifies the pattern for a <see cref="Service{TRequest, TPayload}"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class Microservice : Attribute
{
    public string Pattern { get; }

    public Microservice(string pattern = null)
    {
        this.Pattern = pattern;
    }
}