using microservice.toolkit.core.entity;
using microservice.toolkit.core.extension;

using System;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

public static class MessageMediatorExtensions
{
    public static async Task<ServiceResponse<TPayload>> Send<TRequest, TPayload>(this IMessageMediator messageMediator, Type serviceType, TRequest message)
    {
        return await messageMediator.Send<TPayload>(serviceType.ToPattern(), message);
    }

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TPayload"></typeparam>
    /// <param name="pattern"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static async Task<ServiceResponse<TPayload>> Send<TRequest, TPayload>(this IMessageMediator messageMediator, string pattern, TRequest message)
    {
        return await messageMediator.Send<TPayload>(pattern, message);
    }
}
