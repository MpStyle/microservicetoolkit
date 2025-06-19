using microservice.toolkit.messagemediator.entity;

namespace microservice.toolkit.messagemediator.extension;

public static class ServiceResponseExtension
{
    public static bool IsSuccessful<TPayload>(this ServiceResponse<TPayload> serviceResponse)
    {
        return serviceResponse.Error.HasValue == false;
    }
}
