using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

public interface ISignalEmitter
{
    Task InitAsync(CancellationToken cancellationToken = default);

    Task EmitAsync<TEvent>(string pattern, TEvent message, CancellationToken cancellationToken = default);

    Task ShutdownAsync(CancellationToken cancellationToken);
}