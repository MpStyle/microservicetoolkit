using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

public abstract class SignalHandler<TEvent> : ISignalHandler
{
    public abstract Task Run(TEvent request);

    public async Task Run(object request)
    {
        _ = this.Run((TEvent)request).ConfigureAwait(false);
    }
}

public interface ISignalHandler
{
    Task Run(object request);
}

