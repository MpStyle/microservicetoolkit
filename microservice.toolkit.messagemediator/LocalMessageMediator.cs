using microservice.toolkit.messagemediator.entity;

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
    public Task Init(CancellationToken cancellationToken = default)
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
    public async Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message,
        CancellationToken cancellationToken = default)
    {
        var response = new ServiceResponse<TPayload>();

        try
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException("Pattern must not be null or empty.", nameof(pattern));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var service = serviceFactory(pattern);

            if (service == null)
            {
                throw new ServiceNotFoundException(pattern);
            }

            var serviceResponse = await service.RunAsync(message, cancellationToken);

            if (serviceResponse == null)
            {
                throw new InvalidServiceException(service.GetType().FullName);
            }

            if (serviceResponse.Error.HasValue == false)
            {
                if (serviceResponse.Payload is TPayload payload)
                {
                    response.Payload = payload;
                }
                else
                {
                    logger.LogDebug(
                        $"Payload type mismatch: expected {typeof(TPayload)}, got {serviceResponse.Payload?.GetType()}");
                    response.Error = ServiceError.Unknown;
                }
            }
            else
            {
                response.Error = serviceResponse.Error;
            }
        }
        catch (InvalidServiceException ex)
        {
            logger.LogError(ex, "Invalid service: {Message}", ex.Message);
            response.Error = ServiceError.NullResponse;
        }
        catch (ArgumentNullException ex)
        {
            logger.LogError(ex, "Argument null: {Message}", ex.Message);   
            response.Error = ServiceError.NullRequest;
        } 
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Invalid argument: {Message}", ex.Message);
            response.Error = ServiceError.InvalidPattern;
        }
        catch (ServiceNotFoundException ex)
        {
            logger.LogError(ex, "Service not found: {Pattern}", pattern);
            response.Error = ServiceError.ServiceNotFound;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Generic error while sending message to pattern: {Pattern}", pattern);
            response.Error = ServiceError.Unknown;
        }

        return response;
    }

    /// <summary>
    /// Shuts down the local message mediator.
    /// </summary>
    /// <returns>A task that represents the asynchronous shutdown operation.</returns>
    public Task Shutdown(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}