using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

public abstract class SignalHandler<TEvent> : ISignalHandler
{
    public abstract Task Run(TEvent request);

    public async Task Run(object request)
    {
        await this.Run((TEvent)request);
    }
}

public interface ISignalHandler
{
    Task Run(object request);
}

