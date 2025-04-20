using System;
using System.Threading.Tasks;

namespace microservice.toolkit.core.extension;

public static class SignalEmitterExtensions
{
    public static Task Emit<TEvent>(this ISignalEmitter signalEmitter, Type signalEmitterType, TEvent message)
    {
        return signalEmitter.EmitAsync(signalEmitterType.ToPattern(), message);
    }
}
