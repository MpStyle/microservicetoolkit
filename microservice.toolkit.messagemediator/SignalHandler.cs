using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

/// <summary>
/// Represents a base class for signal handlers that handle a specific type of event.
/// </summary>
/// <typeparam name="TEvent">The type of event to handle.</typeparam>
public abstract class SignalHandler<TEvent> : ISignalHandler
{
    /// <summary>
    /// Runs the signal handler for the specified event.
    /// </summary>
    /// <param name="request">The event to handle.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public abstract Task Run(TEvent request);

    /// <summary>
    /// Runs the signal handler for the specified event.
    /// </summary>
    /// <param name="request">The event to handle.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Run(object request)
    {
        _ = this.Run((TEvent)request).ConfigureAwait(false);
    }
}

/// <summary>
/// Represents a signal handler that can process a specific type of signal.
/// </summary>
public interface ISignalHandler
{
    /// <summary>
    /// Runs the signal handler logic for the specified request.
    /// </summary>
    /// <param name="request">The request object to be processed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Run(object request);
}

