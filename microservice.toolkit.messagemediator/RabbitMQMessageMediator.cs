using microservice.toolkit.core;
using microservice.toolkit.core.entity;

using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator
{
    public class RabbitMQMessageMediator : Disposable, IMessageMediator
    {
        private readonly IModel channel;
        private readonly IConnection connection;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> pendingMessages = new ConcurrentDictionary<string, TaskCompletionSource<byte[]>>();
        private readonly ServiceFactory serviceFactory;
        private readonly RpcMessageMediatorConfiguration configuration;
        private readonly ILogger<RabbitMQMessageMediator> logger;

        public RabbitMQMessageMediator(RpcMessageMediatorConfiguration configuration, ServiceFactory serviceFactory)
            : this(configuration, serviceFactory, new DoNothingLogger<RabbitMQMessageMediator>())
        { }

        public RabbitMQMessageMediator(RpcMessageMediatorConfiguration configuration, ServiceFactory serviceFactory, ILogger<RabbitMQMessageMediator> logger)
        {
            this.serviceFactory = serviceFactory;
            this.configuration = configuration;
            this.logger = logger;

            var factory = new ConnectionFactory()
            {
                HostName = configuration.ConnectionString,
            };

            this.connection = factory.CreateConnection(configuration.ConnectionName);
            this.channel = this.connection.CreateModel();

            // Initialization
            this.channel.BasicQos(0, 1, false);

            this.channel.QueueDeclare(queue: this.configuration.ReplyQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            this.channel.QueueDeclare(queue: this.configuration.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            for (var i = 0; i < configuration.ConsumersPerQueue; i++)
            {
                // Producer
                var producer = new EventingBasicConsumer(channel);
                producer.Received += this.OnProducerReceivesResponse;

                this.channel.BasicConsume(consumer: producer, queue: this.configuration.ReplyQueueName, autoAck: true);

                // Consumer
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += this.OnConsumerReceivesRequest;

                this.channel.BasicConsume(queue: this.configuration.QueueName, autoAck: false, consumer: consumer);
            }
        }

        public async Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message)
        {
            try
            {
                var correlationId = $"{Guid.NewGuid()}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
                var props = channel.CreateBasicProperties();
                props.ReplyTo = this.configuration.ReplyQueueName;
                props.CorrelationId = correlationId;

                var tcs = new TaskCompletionSource<byte[]>(TimeSpan.FromMilliseconds(this.configuration.ResponseTimeout));
                this.pendingMessages.TryAdd(correlationId, tcs);

                var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new RpcMessage
                {
                    Pattern = pattern,
                    Payload = message,
                    RequestType = message.GetType().FullName
                }));
                channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: this.configuration.QueueName,
                    basicProperties: props,
                    body: messageBytes);

                var rawResponse = await tcs.Task;
                var response=JsonSerializer.Deserialize<ServiceResponse<TPayload>>(Encoding.UTF8.GetString(rawResponse));
                
                return response;
            }
            catch (Exception ex)
            {
                this.logger.LogDebug(ex.ToString());
                return new ServiceResponse<TPayload>
                {
                    Error = ErrorCode.UNKNOWN
                };
            }
        }

        private void OnProducerReceivesResponse(object sender, BasicDeliverEventArgs ea)
        {
            var correlationId = ea.BasicProperties.CorrelationId;

            // Check if it is the producer which sent the request
            if (this.pendingMessages.TryRemove(correlationId, out var tcs))
            {
                tcs.SetResult( ea.Body.ToArray());
            }
        }

        private async void OnConsumerReceivesRequest(object sender, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var rpcMessage = JsonSerializer.Deserialize<RpcMessage>(Encoding.UTF8.GetString(body));

            var service = this.serviceFactory(rpcMessage.Pattern);
            var response = new ServiceResponse<object> { Error = ErrorCode.SERVICE_NOT_FOUND };

            if (service != null && rpcMessage.Payload is JsonElement element)
            {
                var json=element.GetRawText();
                var request = JsonSerializer.Deserialize(json, Type.GetType(rpcMessage.RequestType));
                response = await service.Run(request);
            }

            var props = ea.BasicProperties;
            var replyProps = channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));

            channel.BasicPublish(exchange: string.Empty, routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }

        protected override void DisposeUnmanage()
        {
            connection.Close();
            base.DisposeUnmanage();
        }
        
        public Task Shutdown()
        {
            this.DisposeUnmanage();
            return Task.CompletedTask;
        }

        class RpcMessage
        {
            public string Pattern { get; init; }
            public object Payload { get; init; }
            public string RequestType { get; init; }
        }
    }

    public class RpcMessageMediatorConfiguration
    {
        public string ConnectionName { get; init; }
        public string QueueName { get; init; }
        public string ReplyQueueName { get; init; }
        public string ConnectionString { get; init; }
        public uint ConsumersPerQueue { get; init; }

        /// <summary>
        /// Milliseconds
        /// </summary>
        public uint ResponseTimeout { get; set; } = 10000;
    }
}
