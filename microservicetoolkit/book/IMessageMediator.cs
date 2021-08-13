using Microsoft.Extensions.Logging;

using mpstyle.microservice.toolkit.entity;

using System;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book
{
    public delegate IService ServiceFactory(string pattern);

    public interface IMessageMediator
    {
        ServiceFactory ServiceFactory { get; init; }
        ILogger<IMessageMediator> Logger { get; init; }

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="pattern"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<ServiceResponse<TPayload>> Send<TRequest, TPayload>(string pattern, TRequest message)
        {
            try
            {
                var response = await this.Send(pattern, message);
                return new ServiceResponse<TPayload>
                {
                    Error = response.Error,
                    Payload = (TPayload)response.Payload
                };
            }
            catch (Exception ex)
            {
                this.Logger.LogDebug(ex.Message);

                return new ServiceResponse<TPayload>
                {
                    Error = ErrorCode.INVALID_SERVICE_CALL
                };
            }
        }

        Task<ServiceResponse<object>> Send(string pattern, object message);

        public Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message)
        {
            return this.Send(pattern, message) as Task<ServiceResponse<TPayload>>;
        }
    }
}
