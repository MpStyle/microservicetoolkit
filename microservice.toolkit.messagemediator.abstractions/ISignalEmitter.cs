using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

public interface ISignalEmitter
{
    Task Init(CancellationToken cancellationToken = default);

    Task Emit<TEvent>(string pattern, TEvent message, CancellationToken cancellationToken = default);

    Task Shutdown(CancellationToken cancellationToken);
}