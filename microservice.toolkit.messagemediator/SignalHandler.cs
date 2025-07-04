﻿using System.Threading;
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
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public abstract Task Run(TEvent request, CancellationToken cancellationToken);

    /// <summary>
    /// Runs the signal handler for the specified event.
    /// </summary>
    /// <param name="request">The event to handle.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task Run(object request, CancellationToken cancellationToken = default)
    {
        _ = this.Run((TEvent)request, cancellationToken).ConfigureAwait(false);
        return Task.CompletedTask;
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
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Run(object request, CancellationToken cancellationToken);
}