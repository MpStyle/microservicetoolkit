using microservice.toolkit.messagemediator.extension;

using System;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

public interface ISignalEmitter
{
    public Task Emit<TEvent>(Type signalEmitterType, TEvent message)
    {
        return this.Emit(signalEmitterType.ToPattern(), message);
    }

    Task Emit<TEvent>(string pattern, TEvent message);
}
