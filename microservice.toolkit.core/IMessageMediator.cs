using microservice.toolkit.core.entity;

using System.Threading;

using System.Threading.Tasks;

namespace microservice.toolkit.core;

/// <summary>
/// IMessageMediator dispatches request/response messages to a single handler.
/// It dispatches a message to the correct service using pattern.
/// The request-response message style is useful when you need to exchange messages between services.
/// </summary>
public interface IMessageMediator
{
    Task Init(CancellationToken cancellationToken);

    /// <summary>
    /// Sends a generic message.
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    /// <param name="pattern"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message, CancellationToken cancellationToken);

    Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message);

    Task Shutdown(CancellationToken cancellationToken);
}