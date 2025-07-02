using System;

namespace microservice.toolkit.messagemediator;

public class MessageMediatorException(string errorCode) : Exception
{
    public string ErrorCode { get; } = errorCode;

    public override string Message => $"MessageMediator error code: {this.ErrorCode}";
}