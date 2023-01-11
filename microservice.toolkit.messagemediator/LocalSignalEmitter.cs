using microservice.toolkit.core;

using Microsoft.Extensions.Logging;

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

public class LocalSignalEmitter : ISignalEmitter
{
    private readonly SignalHandlerFactory signalHandlerFactory;
    private readonly ILogger<LocalSignalEmitter> logger;

    public LocalSignalEmitter(SignalHandlerFactory serviceFactory, ILogger<LocalSignalEmitter> logger)
    {
        this.signalHandlerFactory = serviceFactory;
        this.logger = logger;
    }

    public async Task Emit<TEvent>(string pattern, TEvent message)
    {
        try
        {
            var handler = this.signalHandlerFactory(pattern);

            if (handler == null)
            {
                throw new SignalHandlerNotFoundException(pattern);
            }

            _ = handler.Run(message).ConfigureAwait(false);
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