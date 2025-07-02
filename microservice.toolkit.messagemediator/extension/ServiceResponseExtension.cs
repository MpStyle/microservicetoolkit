using microservice.toolkit.core.extension;
using microservice.toolkit.messagemediator.entity;

namespace microservice.toolkit.messagemediator.extension;

public static class ServiceResponseExtension
{
    public static bool IsSuccessful<TPayload>(this ServiceResponse<TPayload> serviceResponse)
    {
        return serviceResponse.Error.IsNullOrEmpty();
    }

    public static bool IsError<TPayload>(this ServiceResponse<TPayload> serviceResponse)
    {
        return serviceResponse.Error.IsNotNullOrEmpty();
    }
}