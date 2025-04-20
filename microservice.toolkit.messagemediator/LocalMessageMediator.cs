using microservice.toolkit.core;
using microservice.toolkit.core.entity;

using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

/// <summary>
/// Represents a message mediator that handles local message communication.
/// </summary>
public class LocalMessageMediator(ServiceFactory serviceFactory, ILogger<LocalMessageMediator> logger)
    : IMessageMediator
{
    private readonly ILogger<IMessageMediator> logger = logger;

    public Task InitAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends a message to a service identified by the specified pattern and returns the response.
    /// </summary>
    /// <typeparam name="TPayload">The type of the payload in the response.</typeparam>
    /// <param name="pattern">The pattern used to identify the service.</param>
    /// <param name="message">The message to send to the service.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task result contains a <see cref="ServiceResponse{TPayload}"/> object.</returns>
    public async Task<ServiceResponse<TPayload>> SendAsync<TPayload>(string pattern, object message,
        CancellationToken cancellationToken = default)
    {
        var response = new ServiceResponse<TPayload>();

        try
        {
            var service = serviceFactory(pattern);

            if (service == null)
            {
                throw new ServiceNotFoundException(pattern);
            }

            var serviceResponse = service switch
            {
                IServiceAsync serviceAsync => await serviceAsync.RunAsync(message, cancellationToken),
                IService s => s.Run(message),
                _ => null
            };
            
            if(serviceResponse == null)
            {
                throw new InvalidServiceException(service.GetType().FullName);
            }

            if (serviceResponse.Error.HasValue == false)
            {
                response.Payload = (TPayload)serviceResponse.Payload;
            }
            else
            {
                response.Error = serviceResponse.Error;
            }
        }
        catch (ServiceNotFoundException ex)
        {
            this.logger.LogDebug("Service not found: {Message}", ex.ToString());
            response.Error = ServiceError.ServiceNotFound;
        }
        catch (Exception ex)
        {
            this.logger.LogDebug("Generic error: {Message}", ex.ToString());
            response.Error = ServiceError.Unknown;
        }

        return response;
    }

    /// <summary>
    /// Shuts down the local message mediator.
    /// </summary>
    /// <returns>A task that represents the asynchronous shutdown operation.</returns>
    public Task ShutdownAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

[Serializable]
public class ServiceNotFoundException : Exception
{
    private readonly string pattern;

    /// <summary>
    /// Gets a message that describes the current exception.
    /// </summary>
    public override string Message => $"Service \"{pattern}\" not found";

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceNotFoundException"/> class with a specified pattern.
    /// </summary>
    /// <param name="pattern">The pattern used to search for the service.</param>
    public ServiceNotFoundException(string pattern)
    {
        this.pattern = pattern;
    }
}