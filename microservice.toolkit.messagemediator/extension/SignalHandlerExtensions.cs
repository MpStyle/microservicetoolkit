using microservice.toolkit.core.extension;

namespace microservice.toolkit.messagemediator.extension;

public static class SignalHandlerExtensions
{
    public static string PatternOf<T>() where T : ISignalHandler
    {
        return typeof(T).ToPattern();
    }
}