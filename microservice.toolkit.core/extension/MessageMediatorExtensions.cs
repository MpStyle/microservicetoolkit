using microservice.toolkit.core.entity;

using System;
using System.Threading.Tasks;

namespace microservice.toolkit.core.extension;

public static class MessageMediatorExtensions
{
    public static async Task<ServiceResponse<TPayload>> Send<TRequest, TPayload>(this IMessageMediator messageMediator,
        Type serviceType, TRequest message)
    {
        return await messageMediator.Send<TRequest, TPayload>(serviceType.ToPattern(), message);
    }

    public static async Task<ServiceResponse<TPayload>> Send<TPayload>(this IMessageMediator messageMediator,
        Type serviceType, object message)
    {
        return await messageMediator.Send<TPayload>(serviceType.ToPattern(), message);
    }

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TPayload"></typeparam>
    /// <param name="messageMediator"></param>
    /// <param name="pattern"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static async Task<ServiceResponse<TPayload>> Send<TRequest, TPayload>(this IMessageMediator messageMediator,
        string pattern, TRequest message)
    {
        return await messageMediator.Send<TPayload>(pattern, message);
    }
}