using microservice.toolkit.core;
using microservice.toolkit.core.extension;

using Microsoft.Extensions.Logging;

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

/// <summary>
/// Represents a signal emitter that emits signals locally.
/// </summary>
public class LocalSignalEmitter : ISignalEmitter
{
    private readonly SignalHandlerFactory signalHandlerFactory;
    private readonly ILogger<LocalSignalEmitter> logger;

    public LocalSignalEmitter(SignalHandlerFactory serviceFactory, ILogger<LocalSignalEmitter> logger)
    {
        this.signalHandlerFactory = serviceFactory;
        this.logger = logger;
    }

    public Task Init()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously emits a message to the specified pattern.
    /// </summary>
    /// <typeparam name="TEvent">The type of the message.</typeparam>
    /// <param name="pattern">The pattern to match the message handlers.</param>
    /// <param name="message">The message to emit.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Emit<TEvent>(string pattern, TEvent message)
    {
        try
        {
            var eventHandlers = this.signalHandlerFactory(pattern);

            if (eventHandlers.IsNullOrEmpty())
            {
                throw new SignalHandlerNotFoundException(pattern);
            }

            foreach (var eventHandler in eventHandlers)
            {
                _ = eventHandler.Run(message).ConfigureAwait(false);
            }
        }
        catch (SignalHandlerNotFoundException ex)
        {
            this.logger.LogDebug("Service not found: {Message}", ex.ToString());
        }
        catch (Exception ex)
        {
            this.logger.LogDebug("Generic error: {Message}", ex.ToString());
        }
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

    // Without this constructor, deserialization will fail
    protected SignalHandlerNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public override string Message => $"Signal handler \"{pattern}\" not found";
}