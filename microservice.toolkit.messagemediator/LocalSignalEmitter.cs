using microservice.toolkit.core.extension;

using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

/// <summary>
/// Represents a signal emitter that emits signals locally.
/// </summary>
public class LocalSignalEmitter(SignalHandlerFactory serviceFactory, ILogger<LocalSignalEmitter> logger)
    : ISignalEmitter
{
    public Task Init(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously emits a message to the specified pattern.
    /// </summary>
    /// <typeparam name="TEvent">The type of the message.</typeparam>
    /// <param name="pattern">The pattern to match the message handlers.</param>
    /// <param name="message">The message to emit.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task Emit<TEvent>(string pattern, TEvent message, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventHandlers = serviceFactory(pattern);

            if (eventHandlers.IsNullOrEmpty())
            {
                throw new SignalHandlerNotFoundException(pattern);
            }

            foreach (var eventHandler in eventHandlers)
            {
                _ = eventHandler.Run(message, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (SignalHandlerNotFoundException ex)
        {
            logger.LogDebug("Service not found: {Message}", ex.ToString());
        }
        catch (Exception ex)
        {
            logger.LogDebug("Generic error: {Message}", ex.ToString());
        }

        return Task.CompletedTask;
    }

    public Task Shutdown(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

[Serializable]
public class SignalHandlerNotFoundException : Exception
{
    private readonly string pattern;

    public SignalHandlerNotFoundException(string pattern)
    {
        this.pattern = pattern;
    }

    public SignalHandlerNotFoundException()
    {
    }

    public SignalHandlerNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public override string Message => $"Signal handler \"{pattern}\" not found";
}