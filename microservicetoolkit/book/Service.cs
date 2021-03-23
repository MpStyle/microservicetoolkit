
using Microsoft.Extensions.Logging;

using mpstyle.microservice.toolkit.entity;

using System.Text.Json;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book
{
    public abstract class Service<TRequest, TPayload> : IService
    {
        protected ILogger<IService> Logger { get; }

        protected Service(ILogger<IService> logger)
        {
            this.Logger = logger;
        }

        public abstract Task<ServiceResponse<TPayload>> Run(TRequest request);

        public async Task<string> ORun(string request)
        {
            this.Logger.LogDebug($"Service call - {this.GetType().Name}: {request}");
            return JsonSerializer.Serialize(await this.Run(JsonSerializer.Deserialize<TRequest>(request)));
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

    public interface IService { }
}