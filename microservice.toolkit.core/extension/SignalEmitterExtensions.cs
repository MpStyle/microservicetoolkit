using microservice.toolkit.core.extension;

using System;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

public static class SignalEmitterExtensions
{
    public static Task Emit<TEvent>(this ISignalEmitter signalEmitter, Type signalEmitterType, TEvent message)
    {
        return signalEmitter.Emit(signalEmitterType.ToPattern(), message);
    }
}
