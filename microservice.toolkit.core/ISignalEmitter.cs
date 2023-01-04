using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

public interface ISignalEmitter
{
    Task Emit<TEvent>(string pattern, TEvent message);
}
