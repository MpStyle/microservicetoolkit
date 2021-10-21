using microservice.toolkit.core;
using microservice.toolkit.core.entity;
using microservice.toolkit.messagemediator.entity;

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

        private readonly ServiceBusMessageMediatorConfiguration configuration;
        private readonly ServiceFactory serviceFactory;

        public ServiceBusMessageMediator(ServiceBusMessageMediatorConfiguration configuration,
            ServiceFactory serviceFactory)
            : this(configuration, serviceFactory, new DoNothingLogger<ServiceBusMessageMediator>())
        {
        }

        public ServiceBusMessageMediator(ServiceBusMessageMediatorConfiguration configuration,
            ServiceFactory serviceFactory, ILogger<ServiceBusMessageMediator> logger)
        {
            this.serviceFactory = serviceFactory;
            this.configuration = configuration;

            var connectionStringBuilder = new ServiceBusConnectionStringBuilder(configuration.ConnectionString);
            var connection = connectionStringBuilder.GetNamespaceConnectionString();

            this.requestClient = new QueueClient(connection, configuration.QueueName);
            this.requestClient.RegisterMessageHandler(this.ConsumerMessageHandler, new MessageHandlerOptions(args =>
            {
                logger.LogError(args.Exception.Message);
                return Task.CompletedTask;
            }) { AutoComplete = false, MaxConcurrentCalls = (int)this.configuration.ConsumersPerQueue });

            this.responseSessionClient = new SessionClient(connection, configuration.ReplayQueueName);

            this.responseClient = new QueueClient(connection, configuration.ReplayQueueName);
        }

        public async Task Dispose()
        {
            await this.responseClient.CloseAsync();
            await this.responseSessionClient.CloseAsync();
            await this.requestClient.CloseAsync();
        }

        public async Task Emit<TEvent>(string pattern, TEvent e)
        {
            await this.Send<TEvent>(new BrokeredMessage
            {
                Pattern = pattern, Payload = e, RequestType = e.GetType().FullName, WaitingResponse = false
            }).ConfigureAwait(false);
        }

        public async Task Shutdown()
        {
            await this.Dispose();
        }

        public async Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object request)
        {
            return await this.Send<TPayload>(new BrokeredMessage
            {
                Pattern = pattern,
                Payload = request,
                RequestType = request.GetType().FullName,
                WaitingResponse = true
            });
        }

        private async Task<ServiceResponse<TPayload>> Send<TPayload>(BrokeredMessage rpcMessage)
        {
            var replySessionId = $"{Guid.NewGuid()}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            var session = await this.responseSessionClient.AcceptMessageSessionAsync(replySessionId);

            try
            {
                var rawMessage = JsonSerializer.Serialize(rpcMessage);
                var message = new Message
                {
                    Body = Encoding.UTF8.GetBytes(rawMessage), ReplyToSessionId = replySessionId
                };

                await this.requestClient.SendAsync(message);

                // No waiting for a response (it an event)
                if (!rpcMessage.WaitingResponse)
                {
                    return null;
                }

                // Receive response
                var reply = await session.ReceiveAsync(
                    TimeSpan.FromMilliseconds(this.configuration.ResponseTimeout));
                var response = new ServiceResponse<TPayload> { Error = ErrorCode.InvalidService };

                // Timeout
                if (reply == null)
                {
                    return response;
                }

                response = JsonSerializer.Deserialize<ServiceResponse<TPayload>>(
                    Encoding.UTF8.GetString(reply.Body));
                await session.CompleteAsync(reply.SystemProperties.LockToken);

                return response;
            }
            finally
            {
                await session.CloseAsync(); // release exclusive lock
            }
        }

        private async Task ConsumerMessageHandler(Message message, CancellationToken cancellationToken)
        {
            var requestMessage = Encoding.UTF8.GetString(message.Body);
            var rpcMessage = JsonSerializer.Deserialize<BrokeredMessage>(requestMessage);

            if (rpcMessage == null)
            {
                return;
            }

            var service = this.serviceFactory.Invoke(rpcMessage.Pattern);
            var response = new ServiceResponse<object> { Error = ErrorCode.ServiceNotFound };

            if (service != null && rpcMessage.Payload is JsonElement element)
            {
                var json = element.GetRawText();
                var request = JsonSerializer.Deserialize(json, Type.GetType(rpcMessage.RequestType));
                response = await service.Run(request);
            }

            var replyMessage = new Message
            {
                Body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)),
                SessionId = message.ReplyToSessionId,
                CorrelationId = message.MessageId
            };

            await this.responseClient.SendAsync(replyMessage);
            await this.requestClient.CompleteAsync(message.SystemProperties.LockToken);
        }
    }

    public class ServiceBusMessageMediatorConfiguration
    {
        public string QueueName { get; init; }
        public string ReplayQueueName { get; init; }
        public string ConnectionString { get; init; }
        public uint ConsumersPerQueue { get; init; }

        /// <summary>
        /// Milliseconds
        /// </summary>
        public uint ResponseTimeout { get; set; } = 10000;
    }
}