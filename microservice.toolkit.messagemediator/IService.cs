using microservice.toolkit.core.entity;

namespace microservice.toolkit.messagemediator;

/// <summary>
/// Represents a service that can be executed.
/// </summary>
public interface IService : IBaseService
{
    /// <summary>
    /// Executes the service with the specified request.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <returns>A task that represents the asynchronous operation and contains the service response.</returns>
    ServiceResponse<dynamic> Run(object request);
}