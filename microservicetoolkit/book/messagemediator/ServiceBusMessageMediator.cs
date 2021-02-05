using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using mpstyle.microservice.toolkit.entity;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.messagemediator
{
    public class ServiceBusMessageMediator : IMessageMediator
    {
        private readonly Dictionary<string, Type> services = new Dictionary<string, Type>();
        private readonly ServiceFactory serviceFactory;
        private readonly ILogger<ServiceBusMessageMediator> logger;
        private readonly QueueClient requestClient;
        private readonly SessionClient replySessionClient;
        private readonly RequestReplyHandler requestReplyHandler;
        private Task handlerTask = null;
        private readonly CancellationToken handlerCancellationToken = new CancellationToken();

        public ServiceBusMessageMediator(ServiceMessageMediatorConfiguration configuration, ServiceFactory serviceFactory, ILogger<ServiceBusMessageMediator> logger)
        {
            this.serviceFactory = serviceFactory;
            this.logger = logger;

            var connectionStringBuilder = new ServiceBusConnectionStringBuilder(configuration.ConnectionString);
            var connection = connectionStringBuilder.GetNamespaceConnectionString();
            var requestQueueName = configuration.QueueName;
            var replyQueueName = configuration.ReplayQueueName;
            var consumersPerQueue = configuration.ConsumersPerQueue;

            this.requestClient = new QueueClient(connection, requestQueueName, ReceiveMode.PeekLock);
            this.replySessionClient = new SessionClient(connection, replyQueueName, ReceiveMode.PeekLock);
            this.requestReplyHandler = new RequestReplyHandler(
                this.logger,
                this.requestClient,
                new QueueClient(connection, replyQueueName, ReceiveMode.PeekLock),
                pattern =>
                {
                    this.services.TryGetValue(pattern, out var serviceType);
                    return this.serviceFactory(serviceType);
                },
                consumersPerQueue);
        }

        public IMessageMediator RegisterService(Type service)
        {
            if (this.handlerTask == null)
            {
                this.handlerTask = this.requestReplyHandler.StartAsync(handlerCancellationToken);
            }

            this.services.Add(service.Name, service);
            return this;
        }

        public async Task<ServiceResponse<TPayload>> Send<TRequest, TPayload>(string pattern, TRequest request)
        {
            var replySessionId = Guid.NewGuid().ToString();
            var session = await this.replySessionClient.AcceptMessageSessionAsync(replySessionId);

            try
            {
                var rawMessage = JsonSerializer.Serialize(new ServiceBusMessage
                {
                    Pattern = pattern,
                    Payload = JsonSerializer.Serialize(request)
                });
                var message = new Message
                {
                    Body = Encoding.UTF8.GetBytes(rawMessage),
                    ReplyToSessionId = replySessionId
                };

                this.logger.LogInformation($"------ Message ID: {message.MessageId}; Session ID: {message.ReplyToSessionId}; Instance ID: {Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")}");

                await this.requestClient.SendAsync(message);

                // Receive reply
                var reply = await session.ReceiveAsync(TimeSpan.FromSeconds(10)); // 10s timeout
                var response = new ServiceResponse<TPayload> { Error = ErrorCode.INVALID_SERVICE };

                if (reply != null)
                {
                    response = JsonSerializer.Deserialize<ServiceResponse<TPayload>>(Encoding.UTF8.GetString(reply.Body));
                    await session.CompleteAsync(reply.SystemProperties.LockToken);
                }

                return response;

            }
            finally
            {
                await session.CloseAsync(); // release exlusive lock
            }
        }

        public async void Dispose()
        {
            await this.requestReplyHandler.StopAsync(this.handlerCancellationToken);
            await requestClient.CloseAsync();
            await replySessionClient.CloseAsync();
        }

        class ServiceBusMessage
        {
            public string Pattern { get; set; }
            public string Payload { get; set; }
        }

        class RequestReplyHandler : BackgroundService, IAsyncDisposable
        {
            private readonly ILogger<ServiceBusMessageMediator> logger;
            private readonly IQueueClient incomingQueueClient;
            private readonly IQueueClient outgoingQueueClient;
            private readonly Func<string, IService> serviceBuilder;
            private readonly uint maxConsumer;

            public RequestReplyHandler(ILogger<ServiceBusMessageMediator> logger, IQueueClient incomingQueueClient, IQueueClient outgoingQueueClient, Func<string, IService> serviceBuilder, uint maxConsumer)
            {
                this.logger = logger;
                this.incomingQueueClient = incomingQueueClient;
                this.outgoingQueueClient = outgoingQueueClient;
                this.serviceBuilder = serviceBuilder;
                this.maxConsumer = maxConsumer;
            }

            protected override Task ExecuteAsync(CancellationToken stoppingToken)
            {
                incomingQueueClient.RegisterMessageHandler(async (request, cancellationToken) =>
                {

                    this.logger.LogInformation($"------ Message ID: {request.MessageId}; Session ID: {request.ReplyToSessionId}; Instance ID: {Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")}");

                    var requestMessage = Encoding.UTF8.GetString(request.Body);
                    var rpcMessage = JsonSerializer.Deserialize<ServiceBusMessage>(requestMessage);
                    var service = this.serviceBuilder.Invoke(rpcMessage.Pattern);
                    var response = JsonSerializer.Serialize(new ServiceResponse<object> { Error = ErrorCode.SERVICE_NOT_FOUND });

                    if (service != null)
                    {
                        var method = service.GetType().GetMethod("ORun");

                        if (method != null)
                        {
                            response = await (Task<string>)method.Invoke(service, new object[] { rpcMessage.Payload });
                        }
                    }

                    var replyMessage = new Message
                    {
                        Body = Encoding.UTF8.GetBytes(response),
                        SessionId = request.ReplyToSessionId,
                        CorrelationId = request.MessageId
                    };

                    await outgoingQueueClient.SendAsync(replyMessage);
                    await incomingQueueClient.CompleteAsync(request.SystemProperties.LockToken);
                }, new MessageHandlerOptions(args => {
                    logger.LogError(args.Exception.Message);
                    return Task.CompletedTask;
                })
                {
                    AutoComplete = false,
                    MaxConcurrentCalls = (int)this.maxConsumer
                });

                return Task.CompletedTask;
            }

            public async ValueTask DisposeAsync()
            {
                await incomingQueueClient.CloseAsync();
                await outgoingQueueClient.CloseAsync();
            }
        }
    }

    public class ServiceMessageMediatorConfiguration
    {
        public string QueueName { get; set; }
        public string ReplayQueueName { get; set; }
        public string ConnectionString { get; set; }
        public uint ConsumersPerQueue { get; set; }
    }
}
