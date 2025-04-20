using System;

namespace microservice.toolkit.messagemediator;

[Serializable]
public class InvalidServiceException(string serviceTypeFullName) : Exception
{
    private readonly string serviceTypeFullName = serviceTypeFullName;

    /// <summary>
    /// Gets a message that describes the current exception.
    /// </summary>
    public override string Message => $"Service \"{this.serviceTypeFullName}\" is not supported";
}