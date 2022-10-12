
using microservice.toolkit.core;
using microservice.toolkit.core.entity;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator
{
    public abstract class Service<TRequest, TPayload> : IService
    {
        public abstract Task<ServiceResponse<TPayload>> Run(TRequest request);

        public async Task<ServiceResponse<dynamic>> Run(object request)
        {
            try
            {
                var response = await this.Run((TRequest)request);

                return new ServiceResponse<dynamic> { Error = response.Error, Payload = response.Payload };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return new ServiceResponse<dynamic> { Error = ErrorCode.InvalidServiceExecution };
            }
        }

        protected Task<ServiceResponse<TPayload>> SuccessfulResponseTask(TPayload payload)
        {
            return Task.FromResult(this.SuccessfulResponse(payload));
        }

        protected ServiceResponse<TPayload> SuccessfulResponse(TPayload payload)
        {
            return this.Response(payload, null);
        }

        protected Task<ServiceResponse<TPayload>> UnsuccessfulResponseTask(int error)
        {
            return Task.FromResult(this.UnsuccessfulResponse(error));
        }

        protected ServiceResponse<TPayload> UnsuccessfulResponse(int error)
        {
            return new ServiceResponse<TPayload>()
            {
                Error = error
            };
        }

        protected Task<ServiceResponse<TPayload>> ResponseTask(TPayload payload, int? error)
        {
            return Task.FromResult(this.Response(payload, error));
        }

        protected ServiceResponse<TPayload> Response(TPayload payload, int? error)
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
        Task<ServiceResponse<dynamic>> Run(object request);
    }
}