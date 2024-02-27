using System.Threading.Tasks;

namespace microservice.toolkit.core;

public interface ISignalEmitter
{
    Task Emit<TEvent>(string pattern, TEvent message);
}
