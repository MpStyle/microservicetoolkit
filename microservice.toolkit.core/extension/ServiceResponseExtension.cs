using microservice.toolkit.core.entity;

namespace microservice.toolkit.core.extension;

public static class ServiceResponseExtension
{
    public static bool IsSuccessful<TPayload>(this ServiceResponse<TPayload> serviceResponse)
    {
        return serviceResponse.Error.HasValue == false;
    }
}