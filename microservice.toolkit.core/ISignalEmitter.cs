using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.core;

public interface ISignalEmitter
{
    Task Init(CancellationToken cancellationToken);
    
    Task Emit<TEvent>(string pattern, TEvent message, CancellationToken cancellationToken);
    
    Task Emit<TEvent>(string pattern, TEvent message);

    Task Shutdown(CancellationToken cancellationToken);
}
