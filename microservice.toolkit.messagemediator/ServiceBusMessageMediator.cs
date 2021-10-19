using microservice.toolkit.core;
using microservice.toolkit.core.entity;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator
{
    public class ServiceBusMessageMediator : IMessageMediator
    {
        private readonly QueueClient requestClient;
        private readonly SessionClient responseSessionClient;

        private readonly IQueueClient responseClient;

        private readonly ServiceMessageMediatorConfiguration configuration;
        private readonly ServiceFactory serviceFactory;

        public ServiceBusMessageMediator(ServiceMessageMediatorConfiguration configuration, ServiceFactory serviceFactory)
            : this(configuration, serviceFactory, new DoNothingLogger<ServiceBusMessageMediator>())
        {
        }

        public ServiceBusMessageMediator(ServiceMessageMediatorConfiguration configuration, ServiceFactory serviceFactory, ILogger<ServiceBusMessageMediator> logger)
        {
            this.serviceFactory = serviceFactory;
            this.configuration = configuration;

            var connectionStringBuilder = new ServiceBusConnectionStringBuilder(configuration.ConnectionString);
            var connection = connectionStringBuilder.GetNamespaceConnectionString();

            this.requestClient = new QueueClient(connection, configuration.QueueName, ReceiveMode.PeekLock);
            this.requestClient.RegisterMessageHandler(this.ConsumerMessageHandler, new MessageHandlerOptions(args =>
            {
                logger.LogError(args.Exception.Message);
                return Task.CompletedTask;
            })
            {
                AutoComplete = false,
                MaxConcurrentCalls = (int)this.configuration.ConsumersPerQueue
            });

            this.responseSessionClient = new SessionClient(connection, configuration.ReplayQueueName, ReceiveMode.PeekLock);

            this.responseClient = new QueueClient(connection, configuration.ReplayQueueName, ReceiveMode.PeekLock);
        }

        public async void Dispose()
        {
            await this.responseClient.CloseAsync();
            await this.responseSessionClient.CloseAsync();
            await this.requestClient.CloseAsync();
        }

        public async Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object request)
        {
            var replySessionId = $"{Guid.NewGuid()}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            var session = await this.responseSessionClient.AcceptMessageSessionAsync(replySessionId);

            try
            {
                var rawMessage = JsonSerializer.Serialize(new ServiceBusMessage
                {
                    Pattern = pattern,
                    Payload = request
                });
                var message = new Message
                {
                    Body = Encoding.UTF8.GetBytes(rawMessage),
                    ReplyToSessionId = replySessionId
                };

                await this.requestClient.SendAsync(message);

                // Receive response
                var reply = await session.ReceiveAsync(TimeSpan.FromMilliseconds(this.configuration.ResponseTimeout));
                var response = new ServiceResponse<TPayload> { Error = ErrorCode.INVALID_SERVICE };

                if (reply != null)
                {
                    response = JsonSerializer.Deserialize<ServiceResponse<TPayload>>(Encoding.UTF8.GetString(reply.Body));
                    await session.CompleteAsync(reply.SystemProperties.LockToken);
                }

                return new ServiceResponse<TPayload>
                {
                    Error = response.Error,
                    Payload = (TPayload)response.Payload
                };

            }
            finally
            {
                await session.CloseAsync(); // release exclusive lock
            }
        }

        private async Task ConsumerMessageHandler(Message request, CancellationToken cancellationToken)
        {
            var requestMessage = Encoding.UTF8.GetString(request.Body);
            var rpcMessage = JsonSerializer.Deserialize<ServiceBusMessage>(requestMessage);
            var service = this.serviceFactory.Invoke(rpcMessage.Pattern);
            var response = new ServiceResponse<object> { Error = ErrorCode.SERVICE_NOT_FOUND };

            if (service != null)
            {
                response = await service.Run(rpcMessage.Payload);
            }

            var replyMessage = new Message
            {
                Body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)),
                SessionId = request.ReplyToSessionId,
                CorrelationId = request.MessageId
            };

            await this.responseClient.SendAsync(replyMessage);
            await this.requestClient.CompleteAsync(request.SystemProperties.LockToken);
        }

        class ServiceBusMessage
        {
            public string Pattern { get; set; }
            public object Payload { get; set; }
        }
    }

    public class ServiceMessageMediatorConfiguration
    {
        public string QueueName { get; set; }
        public string ReplayQueueName { get; set; }
        public string ConnectionString { get; set; }
        public uint ConsumersPerQueue { get; set; }

        /// <summary>
        /// Milliseconds
        /// </summary>
        public uint ResponseTimeout { get; set; } = 10000;
    }
}
