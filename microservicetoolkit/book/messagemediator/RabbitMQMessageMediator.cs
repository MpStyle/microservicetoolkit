using Microsoft.Extensions.Logging;

using mpstyle.microservice.toolkit.entity;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.messagemediator
{
    public class RabbitMQMessageMediator : Disposable, IMessageMediator
    {
        private readonly IModel channel;
        private readonly IConnection connection;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> pendingMessages = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
        private readonly ServiceFactory serviceFactory;
        private readonly ILogger<IMessageMediator> logger;
        private readonly RpcMessageMediatorConfiguration configuration;

        public RabbitMQMessageMediator(RpcMessageMediatorConfiguration configuration, ServiceFactory serviceFactory, ILogger<RabbitMQMessageMediator> logger)
        {
            this.logger = logger;
            this.serviceFactory = serviceFactory;
            this.configuration = configuration;

            var factory = new ConnectionFactory()
            {
                Uri = new Uri(configuration.ConnectionString)
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

        public async Task<ServiceResponse<object>> Send(string pattern, object message)
        {
            try
            {
                var correlationId = $"{Guid.NewGuid()}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
                var props = channel.CreateBasicProperties();
                props.ReplyTo = this.configuration.ReplyQueueName;
                props.CorrelationId = correlationId;

                var tcs = new TaskCompletionSource<string>(TimeSpan.FromMilliseconds(this.configuration.ResponseTimeout));

                this.pendingMessages[correlationId] = tcs;

                var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new RpcMessage
                {
                    Pattern = pattern,
                    Payload = message
                }));
                channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: this.configuration.QueueName,
                    basicProperties: props,
                    body: messageBytes);

                var response = await tcs.Task;
                return JsonSerializer.Deserialize<ServiceResponse<object>>(response);
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

        private void OnProducerReceivesResponse(object sender, BasicDeliverEventArgs ea)
        {
            var correlationId = ea.BasicProperties.CorrelationId;

            // It is not the producer who sent the message
            if (this.pendingMessages.TryRemove(correlationId, out var tcs))
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                tcs.SetResult(message);
            }
        }

        private async void OnConsumerReceivesRequest(object sender, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var props = ea.BasicProperties;
            var replyProps = channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            var requestMessage = Encoding.UTF8.GetString(body);
            var rpcMessage = JsonSerializer.Deserialize<RpcMessage>(requestMessage);

            var service = this.serviceFactory(rpcMessage.Pattern);
            var response = new ServiceResponse<object> { Error = ErrorCode.SERVICE_NOT_FOUND };

            if (service != null)
            {
                response = await service.Run(JsonSerializer.Serialize(rpcMessage.Payload));
            }

            var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
            channel.BasicPublish(exchange: string.Empty, routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }

        protected override void DisposeUnmanage()
        {
            connection.Close();
            base.DisposeUnmanage();
        }

        class RpcMessage
        {
            public string Pattern { get; set; }
            public object Payload { get; set; }
        }
    }

    public class RpcMessageMediatorConfiguration
    {
        public string ConnectionName { get; set; }
        public string QueueName { get; set; }
        public string ReplyQueueName { get; set; }
        public string ConnectionString { get; set; }
        public uint ConsumersPerQueue { get; set; }

        /// <summary>
        /// Milliseconds
        /// </summary>
        public uint ResponseTimeout { get; set; } = 10000;
    }
}
