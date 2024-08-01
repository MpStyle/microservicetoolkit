using microservice.toolkit.core.extension;

namespace microservice.toolkit.messagemediator.extension;

public class ServiceExtensions
{
    public static string PatternOf<T>() where T : IService
    {
        return typeof(T).ToPattern();
    }
}