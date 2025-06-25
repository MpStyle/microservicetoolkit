using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.utils;

public static class ServiceUtils
{
    /// <summary>
    /// Creates a task that represents a successful response with the specified payload.
    /// </summary>
    /// <typeparam name="TPayload">The type of the payload.</typeparam>
    /// <param name="payload">The payload to include in the response.</param>
    /// <returns>A task that represents a successful response with the specified payload.</returns>
    public static ServiceResponse<TPayload> SuccessfulResponse<TPayload>(TPayload payload)
    {
        return new ServiceResponse<TPayload> { Payload = payload };
    }

    /// <summary>
    /// Creates a successful service response with the specified payload.
    /// </summary>
    /// <typeparam name="TPayload">The type of the payload.</typeparam>
    /// <param name="payload">The payload to include in the response.</param>
    /// <returns>A <see cref="ServiceResponse{TPayload}"/> representing a successful response.</returns>
    public static Task<ServiceResponse<TPayload>> SuccessfulResponseAsync<TPayload>(TPayload payload)
    {
        return Task.FromResult(new ServiceResponse<TPayload> { Payload = payload });
    }

    /// <summary>
    /// Creates a task that represents an unsuccessful response with the specified error code.
    /// </summary>
    /// <param name="error">The error code.</param>
    /// <returns>A task that represents an unsuccessful response.</returns>
    public static Task<ServiceResponse<TPayload>> UnsuccessfulResponseAsync<TPayload>(int error)
    {
        return Task.FromResult(new ServiceResponse<TPayload> { Error = error });
    }

    /// <summary>
    /// Creates an unsuccessful service response with the specified error code.
    /// </summary>
    /// <param name="error">The error code.</param>
    /// <returns>An instance of <see cref="ServiceResponse{TPayload}"/> representing an unsuccessful response.</returns>
    public static ServiceResponse<TPayload> UnsuccessfulResponse<TPayload>(int error)
    {
        return new ServiceResponse<TPayload> { Error = error };
    }

    /// <summary>
    /// Creates a task that represents the completion of the <see cref="Response"/> method with the specified payload and error code.
    /// </summary>
    /// <typeparam name="TPayload">The type of the payload.</typeparam>
    /// <param name="payload">The payload to be included in the response.</param>
    /// <param name="error">The error code to be included in the response.</param>
    /// <returns>A task that represents the completion of the <see cref="Response"/> method with the specified payload and error code.</returns>
    public static Task<ServiceResponse<TPayload>> ResponseAsync<TPayload>(TPayload payload, int? error)
    {
        return Task.FromResult(error.HasValue ? new ServiceResponse<TPayload> { Error = error.Value } : new ServiceResponse<TPayload> { Payload = payload });
    }

    /// <summary>
    /// Creates a new instance of <see cref="ServiceResponse{TPayload}"/> with the specified payload and error.
    /// </summary>
    /// <typeparam name="TPayload">The type of the payload.</typeparam>
    /// <param name="payload">The payload value.</param>
    /// <param name="error">The error value.</param>
    /// <returns>A new instance of <see cref="ServiceResponse{TPayload}"/> with the specified payload and error.</returns>
    public static ServiceResponse<TPayload> Response<TPayload>(TPayload payload, int? error)
    {
        return error.HasValue ? new ServiceResponse<TPayload> { Error = error } : new ServiceResponse<TPayload> { Payload = payload };
    }
}