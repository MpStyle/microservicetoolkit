using System;

namespace microservice.toolkit.messagemediator;

public class SignalEmitterException(string errorCode) : Exception
{
    public override string Message => $"SignalEmitter error code: {errorCode}";
}