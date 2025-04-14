using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.core;

public interface ISignalEmitter
{
    Task Init(CancellationToken cancellationToken);
    
    Task Init();
    
    Task Emit<TEvent>(string pattern, TEvent message, CancellationToken cancellationToken);
    
    Task Emit<TEvent>(string pattern, TEvent message);
}
