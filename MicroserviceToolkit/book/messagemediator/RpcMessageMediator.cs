using Microsoft.Extensions.Logging;

using mpstyle.microservice.toolkit.entity;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.messagemediator
{
    public class RpcMessageMediator : Disposable, IMessageMediator
    {
        private readonly Dictionary<string, Type> services = new Dictionary<string, Type>();
        private readonly IModel channel;
        private readonly IConnection connection;
        private readonly RpcClient rpcClient;
        private readonly ServiceFactory serviceFactory;
        private readonly ILogger<RpcMessageMediator> logger;

        public RpcMessageMediator(IConfigurationManager configurationManager, ServiceFactory serviceFactory, ILogger<RpcMessageMediator> logger)
        {
            this.logger = logger;
            this.serviceFactory = serviceFactory;
            this.rpcClient = new RpcClient(configurationManager);

            var requestQueueName = configurationManager.GetString(SettingKey.Rpc.QUEUE_NAME);
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(configurationManager.GetString(SettingKey.Rpc.CONNECTION_STRING))
            };

            this.connection = factory.CreateConnection("tvguide-api-server");
            this.channel = connection.CreateModel();

            this.channel.QueueDeclare(queue: requestQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            this.channel.BasicQos(0, 1, false);

            for (var i = 0; i < configurationManager.GetInt(SettingKey.Rpc.CONSUMERS_PER_QUEUE); i++)
            {
                var consumer = new EventingBasicConsumer(channel);
                this.channel.BasicConsume(queue: requestQueueName, autoAck: false, consumer: consumer);
                consumer.Received += this.ConsumerReceived;
            }
        }

        public IMessageMediator RegisterService(Type service)
        {
            this.services.Add(service.Name, service);
            return this;
        }

        public async Task<ServiceResponse<TPayload>> Send<TRequest, TPayload>(string pattern, TRequest message)
        {
            try
            {
                var service = this.services[pattern];
                if (service == null || (service is Service<TRequest, TPayload>) == false)
                {
                    return new ServiceResponse<TPayload> { Error = ErrorCode.INVALID_SERVICE };
                }

                var response = await rpcClient.SendAsync(new RpcMessage
                {
                    Pattern = pattern,
                    Payload = JsonSerializer.Serialize(message)
                });

                return JsonSerializer.Deserialize<ServiceResponse<TPayload>>(response);
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

        private async void ConsumerReceived(object sender, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var props = ea.BasicProperties;
            var replyProps = channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            var requestMessage = Encoding.UTF8.GetString(body);
            var rpcMessage = JsonSerializer.Deserialize<RpcMessage>(requestMessage);

            this.services.TryGetValue(rpcMessage.Pattern, out var serviceType);

            var service = this.serviceFactory(serviceType);
            var response = JsonSerializer.Serialize(new ServiceResponse<object> { Error = ErrorCode.SERVICE_NOT_FOUND });

            if (service != null)
            {
                var method = service.GetType().GetMethod("ORun");

                if (method != null)
                {
                    response = await (Task<string>)method.Invoke(service, new object[] { rpcMessage.Payload });
                }
            }

            var responseBytes = Encoding.UTF8.GetBytes(response);
            channel.BasicPublish(exchange: string.Empty, routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }

        protected override void DisposeUnmanage()
        {
            connection.Close();
            base.DisposeUnmanage();
        }
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

        public RpcClient(IConfigurationManager configurationManager)
        {
            this.queueName = configurationManager.GetString(SettingKey.Rpc.QUEUE_NAME);
            this.replyQueueName = configurationManager.GetString(SettingKey.Rpc.REPLY_QUEUE_NAME);

            var factory = new ConnectionFactory()
            {
                Uri = new Uri(configurationManager.GetString(SettingKey.Rpc.CONNECTION_STRING))
            };

            this.connection = factory.CreateConnection("tvguide-api-client");
            this.channel = this.connection.CreateModel();

            this.channel.QueueDeclare(queue: replyQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            this.channel.BasicQos(0, 1, false);

            for (var i = 0; i < configurationManager.GetInt(SettingKey.Rpc.CONSUMERS_PER_QUEUE); i++)
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
            var correlationId = $"{Guid.NewGuid()}-{Guid.NewGuid()}-{DateTime.Now.ToEpoch()}";
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
