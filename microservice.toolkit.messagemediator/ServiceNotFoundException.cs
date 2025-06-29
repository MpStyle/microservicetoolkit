using System;

namespace microservice.toolkit.messagemediator;

[Serializable]
public class ServiceNotFoundException : Exception
{
    private readonly string pattern;

    /// <summary>
    /// Gets a message that describes the current exception.
    /// </summary>
    public override string Message => $"Service \"{this.pattern}\" not found";

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceNotFoundException"/> class with a specified pattern.
    /// </summary>
    /// <param name="pattern">The pattern used to search for the service.</param>
    public ServiceNotFoundException(string pattern)
    {
        this.pattern = pattern;
    }
}