using System.Threading.Tasks;

namespace microservice.toolkit.core;

public interface ISignalEmitter
{
    Task Init();
    
    Task Emit<TEvent>(string pattern, TEvent message);
}
