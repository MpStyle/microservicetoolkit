using Microsoft.Extensions.Logging;

using mpstyle.microservice.toolkit.entity;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.messagemediator
{
    public class LocalMessageMediator : IMessageMediator
    {
        private readonly ServiceFactory serviceFactory;
        private readonly ILogger<LocalMessageMediator> logger;
        private readonly Dictionary<string, Type> services = new Dictionary<string, Type>();

        public LocalMessageMediator(ServiceFactory serviceFactory, ILogger<LocalMessageMediator> logger)
        {
            this.serviceFactory = serviceFactory;
            this.logger = logger;
        }

        public IMessageMediator RegisterService(Type service)
        {
            services.Add(service.Name, service);
            return this;
        }

        public Task<ServiceResponse<TResponse>> Send<TRequest, TResponse>(string pattern, TRequest message)
        {
            try
            {
                services.TryGetValue(pattern, out Type serviceType);
                var service = serviceFactory(serviceType);

                if (service == null)
                {
                    throw new ServiceNotFoundException(pattern);
                }

                if (service is Service<TRequest, TResponse> callableService)
                {
                    return callableService.Run(message);
                }

                throw new ServiceNotFoundException(pattern);
            }
            catch (ServiceNotFoundException ex)
            {
                logger.LogDebug(ex.ToString());
                return Task.FromResult(new ServiceResponse<TResponse>
                {
                    Error = ErrorCode.SERVICE_NOT_FOUND
                });
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.ToString());
                return Task.FromResult(new ServiceResponse<TResponse>
                {
                    Error = ErrorCode.UNKNOWN
                });
            }
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
