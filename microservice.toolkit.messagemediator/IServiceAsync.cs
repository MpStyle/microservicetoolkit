using microservice.toolkit.core.entity;

using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

/// <summary>
/// Represents a service that can be executed.
/// </summary>
public interface IServiceAsync : IBaseService
{
    /// <summary>
    /// Executes the service with the specified request.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represents the asynchronous operation and contains the service response.</returns>
    Task<ServiceResponse<dynamic>> RunAsync(object request, CancellationToken cancellationToken = default);
}