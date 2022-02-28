
using microservice.toolkit.core.entity;

using System.Threading.Tasks;

namespace microservice.toolkit.core
{
    public interface IMessageMediator
    {
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
        /// Sends a message without specifying only the type of output parameter.
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="pattern"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message);

        void Emit<TEvent>(string pattern, TEvent e);

        Task Shutdown();
    }
}
