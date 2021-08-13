
using mpstyle.microservice.toolkit.entity;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book
{
    public abstract class Service<TRequest, TPayload> : IService
    {
        public abstract Task<ServiceResponse<TPayload>> Run(TRequest request);

        public async Task<ServiceResponse<object>> Run(object request)
        {
            try
            {
                var response = await this.Run((TRequest)request);

                return new ServiceResponse<object> { Error = response.Error, Payload = response.Payload };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return new ServiceResponse<object> { Error = ErrorCode.INVALID_SERVICE_EXECUTION };
            }
        }

        public ServiceResponse<TPayload> SuccessfulResponse(TPayload payload)
        {
            return new ServiceResponse<TPayload>()
            {
                Payload = payload
            };
        }

        public ServiceResponse<TPayload> UnsuccessfulResponse(int error)
        {
            return new ServiceResponse<TPayload>()
            {
                Error = error
            };
        }

        public ServiceResponse<TPayload> Response(TPayload payload, int? error)
        {
            return new ServiceResponse<TPayload>()
            {
                Payload = payload,
                Error = error
            };
        }
    }

    public interface IService
    {
        Task<ServiceResponse<object>> Run(object request);
    }
}