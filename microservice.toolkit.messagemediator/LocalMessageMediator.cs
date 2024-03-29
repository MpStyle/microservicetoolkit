﻿using microservice.toolkit.core;
using microservice.toolkit.core.entity;

using Microsoft.Extensions.Logging;

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator
{
    /// <summary>
    /// Represents a message mediator that handles local message communication.
    /// </summary>
    public class LocalMessageMediator : IMessageMediator
    {
        private readonly ServiceFactory serviceFactory;
        private readonly ILogger<IMessageMediator> logger;

        public LocalMessageMediator(ServiceFactory serviceFactory, ILogger<LocalMessageMediator> logger)
        {
            this.serviceFactory = serviceFactory;
            this.logger = logger;
        }

        /// <summary>
        /// Sends a message to a service identified by the specified pattern and returns the response.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload in the response.</typeparam>
        /// <param name="pattern">The pattern used to identify the service.</param>
        /// <param name="message">The message to send to the service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task result contains a <see cref="ServiceResponse{TPayload}"/> object.</returns>
        public async Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message)
        {
            try
            {
                var service = this.serviceFactory(pattern);

                if (service == null)
                {
                    throw new ServiceNotFoundException(pattern);
                }

                var response = await service.Run(message);
                return new ServiceResponse<TPayload>
                {
                    Error = response.Error,
                    Payload = (TPayload)response.Payload
                };
            }
            catch (ServiceNotFoundException ex)
            {
                this.logger.LogDebug("Service not found: {Message}", ex.ToString());
                return new ServiceResponse<TPayload>
                {
                    Error = ErrorCode.ServiceNotFound
                };
            }
            catch (Exception ex)
            {
                this.logger.LogDebug("Generic error: {Message}", ex.ToString());
                return new ServiceResponse<TPayload>
                {
                    Error = ErrorCode.Unknown
                };
            }
        }

        /// <summary>
        /// Shuts down the local message mediator.
        /// </summary>
        /// <returns>A task that represents the asynchronous shutdown operation.</returns>
        public Task Shutdown()
        {
            return Task.CompletedTask;
        }
    }

    [Serializable]
    public class ServiceNotFoundException : Exception
    {
        private readonly string pattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNotFoundException"/> class with a specified pattern.
        /// </summary>
        /// <param name="pattern">The pattern used to search for the service.</param>
        public ServiceNotFoundException(string pattern)
        {
            this.pattern = pattern;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNotFoundException"/> class.
        /// </summary>
        public ServiceNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNotFoundException"/> class with a specified message and inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ServiceNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNotFoundException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected ServiceNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message => $"Service \"{pattern}\" not found";
    }
}
