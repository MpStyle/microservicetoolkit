using Microsoft.Extensions.Logging;

using mpstyle.microservice.toolkit.entity;

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.messagemediator
{
    public class LocalMessageMediator : IMessageMediator
    {
        public ServiceFactory ServiceFactory { get; init; }
        public ILogger<IMessageMediator> Logger { get; init; }

        public LocalMessageMediator(ILogger<LocalMessageMediator> logger, ServiceFactory serviceFactory)
        {
            this.ServiceFactory = serviceFactory;
            this.Logger = logger;
        }

        public async Task<ServiceResponse<object>> Send(string pattern, object message)
        {
            try
            {
                var service = this.ServiceFactory(pattern);

                if (service == null)
                {
                    throw new ServiceNotFoundException(pattern);
                }

                return await service.Run(message);
            }
            catch (ServiceNotFoundException ex)
            {
                this.Logger.LogDebug(ex.ToString());
                return new ServiceResponse<object>
                {
                    Error = ErrorCode.SERVICE_NOT_FOUND
                };
            }
            catch (Exception ex)
            {
                this.Logger.LogDebug(ex.ToString());
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
