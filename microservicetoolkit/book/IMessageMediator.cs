using mpstyle.microservice.toolkit.entity;

using System;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book
{
    public delegate IService ServiceFactory(Type type);

    public interface IMessageMediator
    {
        IMessageMediator RegisterService(Type service);

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="pattern"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<ServiceResponse<TPayload>> Send<TRequest, TPayload>(string pattern, TRequest message);
    }
}
