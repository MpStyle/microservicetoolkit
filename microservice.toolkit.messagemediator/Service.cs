using microservice.toolkit.core.entity;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator
{
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
        /// <returns>A task representing the asynchronous operation. The task result contains the service response.</returns>
        public abstract Task<ServiceResponse<TPayload>> Run(TRequest request);

        /// <summary>
        /// Executes the service logic with the specified request.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task result contains a <see cref="ServiceResponse{T}"/> object.</returns>
        public async Task<ServiceResponse<dynamic>> Run(object request)
        {
            try
            {
                var response = await this.Run((TRequest)request);

                return new ServiceResponse<dynamic> { Error = response.Error, Payload = response.Payload };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return new ServiceResponse<dynamic> { Error = ServiceError.InvalidServiceExecution };
            }
        }

        /// <summary>
        /// Creates a task that represents a successful response with the specified payload.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload.</typeparam>
        /// <param name="payload">The payload to include in the response.</param>
        /// <returns>A task that represents a successful response with the specified payload.</returns>
        protected Task<ServiceResponse<TPayload>> SuccessfulResponseTask(TPayload payload)
        {
            return Task.FromResult(this.SuccessfulResponse(payload));
        }

        /// <summary>
        /// Creates a successful service response with the specified payload.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload.</typeparam>
        /// <param name="payload">The payload to include in the response.</param>
        /// <returns>A <see cref="ServiceResponse{TPayload}"/> representing a successful response.</returns>
        protected ServiceResponse<TPayload> SuccessfulResponse(TPayload payload)
        {
            return this.Response(payload, null);
        }

        /// <summary>
        /// Creates a task that represents an unsuccessful response with the specified error code.
        /// </summary>
        /// <param name="error">The error code.</param>
        /// <returns>A task that represents an unsuccessful response.</returns>
        protected Task<ServiceResponse<TPayload>> UnsuccessfulResponseTask(int error)
        {
            return Task.FromResult(this.UnsuccessfulResponse(error));
        }

        /// <summary>
        /// Creates an unsuccessful service response with the specified error code.
        /// </summary>
        /// <param name="error">The error code.</param>
        /// <returns>An instance of <see cref="ServiceResponse{TPayload}"/> representing an unsuccessful response.</returns>
        protected ServiceResponse<TPayload> UnsuccessfulResponse(int error)
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
        protected Task<ServiceResponse<TPayload>> ResponseTask(TPayload payload, int? error)
        {
            return Task.FromResult(this.Response(payload, error));
        }

        /// <summary>
        /// Creates a new instance of <see cref="ServiceResponse{TPayload}"/> with the specified payload and error.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload.</typeparam>
        /// <param name="payload">The payload value.</param>
        /// <param name="error">The error value.</param>
        /// <returns>A new instance of <see cref="ServiceResponse{TPayload}"/> with the specified payload and error.</returns>
        protected ServiceResponse<TPayload> Response(TPayload payload, int? error)
        {
            if (error.HasValue)
            {
                return new ServiceResponse<TPayload> { Error = error };
            }

            return new ServiceResponse<TPayload> { Payload = payload };
        }
    }

    /// <summary>
    /// Represents a service that can be executed.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Executes the service with the specified request.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>A task that represents the asynchronous operation and contains the service response.</returns>
        Task<ServiceResponse<dynamic>> Run(object request);
    }
}