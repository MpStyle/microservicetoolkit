using Microsoft.Extensions.Logging;

using mpstyle.microservice.toolkit.entity;

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.messagemediator
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

        public async Task<ServiceResponse<object>> Send(string pattern, object message)
        {
            try
            {
                var service = this.serviceFactory(pattern);

                if (service == null)
                {
                    throw new ServiceNotFoundException(pattern);
                }

                return await service.Run(message);
            }
            catch (ServiceNotFoundException ex)
            {
                this.logger.LogDebug(ex.ToString());
                return new ServiceResponse<object>
                {
                    Error = ErrorCode.SERVICE_NOT_FOUND
                };
            }
            catch (Exception ex)
            {
                this.logger.LogDebug(ex.ToString());
                return new ServiceResponse<object>
                {
                    Error = ErrorCode.UNKNOWN
                };
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
