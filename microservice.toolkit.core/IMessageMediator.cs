using microservice.toolkit.core.entity;
using microservice.toolkit.core.extension;

using System;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator
{
    /// <summary>
    /// IMessageMediator dispatches request/response messages to a single handler.
    /// It dispatches a message to the correct service using pattern.
    /// The request-response message style is useful when you need to exchange messages between services.
    /// </summary>
    public partial interface IMessageMediator
    {
        public async Task<ServiceResponse<TPayload>> Send<TRequest, TPayload>(Type serviceType, TRequest message)
        {
            return await this.Send<TPayload>(serviceType.ToPattern(), message);
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="pattern"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<ServiceResponse<TPayload>> Send<TRequest, TPayload>(string pattern, TRequest message)
        {
            return await this.Send<TPayload>(pattern, message);
        }

        /// <summary>
        /// Sends a generic message.
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="pattern"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message);

        Task Shutdown();
    }
}
