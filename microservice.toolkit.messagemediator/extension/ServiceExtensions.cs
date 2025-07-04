using microservice.toolkit.messagemediator.entity;
using microservice.toolkit.messagemediator.utils;

using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.extension;

public static class ServiceExtensions
{
    public static ServiceResponse<TPayload> SuccessfulResponse<TPayload>(this IService _, TPayload payload)
    {
        return ServiceUtils.SuccessfulResponse(payload);
    }
    
    public static ServiceResponse<TPayload> SuccessfulResponse<TRequest, TPayload>(this Service<TRequest, TPayload> _, TPayload payload)
    {
        return ServiceUtils.SuccessfulResponse(payload);
    }

    public static Task<ServiceResponse<TPayload>> SuccessfulResponseAsync<TPayload>(this IService _, TPayload payload)
    {
        return ServiceUtils.SuccessfulResponseAsync(payload);
    }
    
    public static Task<ServiceResponse<TPayload>> SuccessfulResponseAsync<TRequest, TPayload>(this Service<TRequest, TPayload> _, TPayload payload)
    {
        return ServiceUtils.SuccessfulResponseAsync(payload);
    }

    public static Task<ServiceResponse<TPayload>> UnsuccessfulResponseAsync<TPayload>(this IService _, string error)
    {
        return ServiceUtils.UnsuccessfulResponseAsync<TPayload>(error);
    }
    
    public static Task<ServiceResponse<TPayload>> UnsuccessfulResponseAsync<TResponse, TPayload>(this Service<TResponse, TPayload> _, string error)
    {
        return ServiceUtils.UnsuccessfulResponseAsync<TPayload>(error);
    }

    public static ServiceResponse<TPayload> UnsuccessfulResponse<TPayload>(this IService _, string error)
    {
        return ServiceUtils.UnsuccessfulResponse<TPayload>(error);
    }
    
    public static ServiceResponse<TPayload> UnsuccessfulResponse<TRequest, TPayload>(this Service<TRequest, TPayload> _, string error)
    {
        return ServiceUtils.UnsuccessfulResponse<TPayload>(error);
    }

    public static Task<ServiceResponse<TPayload>> ResponseAsync<TPayload>(this IService _, TPayload payload, string? error)
    {
        return ServiceUtils.ResponseAsync(payload, error);
    }
    
    public static Task<ServiceResponse<TPayload>> ResponseAsync<TRequest, TPayload>(this Service<TRequest, TPayload> _, TPayload payload, string? error)
    {
        return ServiceUtils.ResponseAsync(payload, error);
    }

    public static ServiceResponse<TPayload> Response<TPayload>(this IService _, TPayload payload, string? error)
    {
        return ServiceUtils.Response(payload, error);
    }
    
    public static ServiceResponse<TPayload> Response<TRequest, TPayload>(this Service<TRequest, TPayload> _, TPayload payload, string? error)
    {
        return ServiceUtils.Response(payload, error);
    }
}