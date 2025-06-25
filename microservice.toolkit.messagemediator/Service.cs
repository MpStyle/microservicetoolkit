using microservice.toolkit.core.entity;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

/// <summary>
/// Represents an abstract base class for a service that handles requests and returns a response.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TPayload">The type of the response payload.</typeparam>
public abstract class Service<TRequest, TPayload> : IService
{
    /// <summary>
    /// Executes the service logic for the specified request.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TPayload">The type of the response payload.</typeparam>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation. The task result contains the service response.</returns>
    public abstract Task<ServiceResponse<TPayload>> RunAsync(
        TRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Executes the service logic with the specified request.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task result contains a <see cref="ServiceResponse{T}"/> object.</returns>
    public async Task<ServiceResponse<dynamic>> RunAsync(object request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await this.RunAsync((TRequest)request, cancellationToken);

            return new ServiceResponse<dynamic> { Error = response.Error, Payload = response.Payload };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            return new ServiceResponse<dynamic> { Error = ServiceError.InvalidServiceExecution };
        }
    }

    public async Task<ServiceResponse<dynamic>> RunAsync(object request)
    {
        return await this.RunAsync(request, CancellationToken.None);
    }
}