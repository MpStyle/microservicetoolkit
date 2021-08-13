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
        private readonly RpcClient rpcClient;

        public ServiceFactory ServiceFactory { get; init; }
        public ILogger<IMessageMediator> Logger { get; init; }

        public RabbitMQMessageMediator(RpcMessageMediatorConfiguration configuration, ServiceFactory serviceFactory, ILogger<RabbitMQMessageMediator> logger)
        {
            this.Logger = logger;
            this.ServiceFactory = serviceFactory;
            this.rpcClient = new RpcClient(configuration);

            var requestQueueName = configuration.QueueName;
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(configuration.ConnectionString)
            };

            this.connection = factory.CreateConnection(configuration.ConnectionName);
            this.channel = connection.CreateModel();

            this.channel.QueueDeclare(queue: requestQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            this.channel.BasicQos(0, 1, false);

            for (var i = 0; i < configuration.ConsumersPerQueue; i++)
            {
                var consumer = new EventingBasicConsumer(channel);
                this.channel.BasicConsume(queue: requestQueueName, autoAck: false, consumer: consumer);
                consumer.Received += this.ConsumerReceived;
            }
        }

        public async Task<ServiceResponse<object>> Send(string pattern, object message)
        {
            try
            {
                var response = await rpcClient.SendAsync(new RpcMessage
                {
                    Pattern = pattern,
                    Payload = JsonSerializer.Serialize(message)
                });

                return JsonSerializer.Deserialize<ServiceResponse<object>>(response);
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

        private async void ConsumerReceived(object sender, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var props = ea.BasicProperties;
            var replyProps = channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            var requestMessage = Encoding.UTF8.GetString(body);
            var rpcMessage = JsonSerializer.Deserialize<RpcMessage>(requestMessage);

            var service = this.ServiceFactory(rpcMessage.Pattern);
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
            public string Payload { get; set; }
        }

        class RpcClient : Disposable
        {
            private readonly IConnection connection;
            private readonly IModel channel;
            private readonly string queueName;
            private readonly string replyQueueName;
            private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> pendingMessages = new ConcurrentDictionary<string, TaskCompletionSource<string>>();

            public RpcClient(RpcMessageMediatorConfiguration configuration)
            {
                this.queueName = configuration.QueueName;
                this.replyQueueName = configuration.ReplayQueueName;

                var factory = new ConnectionFactory()
                {
                    Uri = new Uri(configuration.ConnectionString)
                };

                this.connection = factory.CreateConnection(configuration.ConnectionName);
                this.channel = this.connection.CreateModel();

                this.channel.QueueDeclare(queue: replyQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                this.channel.BasicQos(0, 1, false);

                for (var i = 0; i < configuration.ConsumersPerQueue; i++)
                {
                    var consumer = new EventingBasicConsumer(channel);

                    this.channel.BasicConsume(
                        consumer: consumer,
                        queue: replyQueueName,
                        autoAck: true);

                    consumer.Received += (model, ea) =>
                    {
                        var correlationId = ea.BasicProperties.CorrelationId;
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        this.pendingMessages.TryRemove(correlationId, out var tcs);

                        if (tcs != null)
                        {
                            tcs.SetResult(message);
                        }
                    };
                }
            }

            public async Task<string> SendAsync(RpcMessage message)
            {
                var correlationId = $"{Guid.NewGuid()}-{Guid.NewGuid()}-{DateTime.UtcNow.ToEpoch()}";
                var props = channel.CreateBasicProperties();
                props.ReplyTo = this.replyQueueName;
                props.CorrelationId = correlationId;

                var tcs = new TaskCompletionSource<string>();

                this.pendingMessages[correlationId] = tcs;

                var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: this.queueName,
                    basicProperties: props,
                    body: messageBytes);

                return await tcs.Task;
            }

            protected override void DisposeUnmanage()
            {
                connection.Close();
                base.DisposeUnmanage();
            }
        }
    }

    public class RpcMessageMediatorConfiguration
    {
        public string ConnectionName { get; set; }
        public string QueueName { get; set; }
        public string ReplayQueueName { get; set; }
        public string ConnectionString { get; set; }
        public uint ConsumersPerQueue { get; set; }
    }
}
