
using mpstyle.microservice.toolkit.entity;

using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book
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
        public Task<ServiceResponse<TPayload>> Send<TRequest, TPayload>(string pattern, TRequest message)
        {
            return this.Send(pattern, message) as Task<ServiceResponse<TPayload>>;
        }

        /// <summary>
        /// Sends a message without specifying the type of input and output parameters.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<ServiceResponse<object>> Send(string pattern, object message);

        /// <summary>
        /// Sends a message without specifying only the type of output parameter.
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="pattern"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message)
        {
            return this.Send(pattern, message) as Task<ServiceResponse<TPayload>>;
        }
    }
}
