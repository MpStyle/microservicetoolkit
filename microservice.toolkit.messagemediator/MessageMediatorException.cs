using System;

namespace microservice.toolkit.messagemediator;

public class MessageMediatorException(int errorCode) : Exception
{
    public int ErrorCode { get; } = errorCode;

    public override string Message => $"MessageMediator error code: {this.ErrorCode}";
}