using microservice.toolkit.core;
using microservice.toolkit.core.entity;

using Microsoft.Extensions.Logging;

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator
{
    public class LocalMessageMediator : IMessageMediator
    {
        private readonly ServiceFactory serviceFactory;
        private readonly ILogger<IMessageMediator> logger = new DoNothingLogger<IMessageMediator>();

        public LocalMessageMediator(ServiceFactory serviceFactory)
        {
            this.serviceFactory = serviceFactory;
        }

        public LocalMessageMediator(ServiceFactory serviceFactory, ILogger<LocalMessageMediator> logger)
        {
            this.serviceFactory = serviceFactory;
            this.logger = logger;
        }

        public async Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message)
        {
            try
            {
                var service = this.serviceFactory(pattern);

                if (service == null)
                {
                    throw new ServiceNotFoundException(pattern);
                }

                var response = await service.Run(message);
                return new ServiceResponse<TPayload>
                {
                    Error = response.Error,
                    Payload = (TPayload)response.Payload
                };
            }
            catch (ServiceNotFoundException ex)
            {
                this.logger.LogDebug(ex.ToString());
                return new ServiceResponse<TPayload>
                {
                    Error = ErrorCode.ServiceNotFound
                };
            }
            catch (Exception ex)
            {
                this.logger.LogDebug(ex.ToString());
                return new ServiceResponse<TPayload>
                {
                    Error = ErrorCode.Unknown
                };
            }
        }

        public void Emit<TEvent>(string pattern, TEvent e)
        {
            this.Send<TEvent>(pattern, e).ConfigureAwait(false);
        }

        public Task Shutdown()
        {
            return Task.CompletedTask;
        }
    }

    [Serializable]
    public class ServiceNotFoundException : Exception
    {
        private readonly string pattern;

        public ServiceNotFoundException(string pattern)
        {
            this.pattern = pattern;
        }

        public ServiceNotFoundException()
        {
        }

        public ServiceNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        // Without this constructor, deserialization will fail
        protected ServiceNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string Message => $"Service \"{pattern}\" not found";
    }
}
