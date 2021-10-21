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
    public class RabbitMQMessageMediator : IMessageMediator, IDisposable
    {
        private readonly ILogger logger;
        private readonly RabbitMQMessageMediatorConfiguration configuration;
        private readonly ServiceFactory serviceFactory;

        private readonly IConnection connection;
        private readonly IModel consumerChannel;
        private readonly IModel producerChannel;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> pendingMessages = new();

        public RabbitMQMessageMediator(RabbitMQMessageMediatorConfiguration configuration,
            ServiceFactory serviceFactory, ILogger logger = null)
        {
            this.configuration = configuration;
            this.serviceFactory = serviceFactory;
            this.logger = logger ?? new DoNothingLogger<RabbitMQMessageMediator>();

            var factory = new ConnectionFactory() { HostName = this.configuration.ConnectionString };
            this.connection = factory.CreateConnection();

            // Consumer
            this.consumerChannel = connection.CreateModel();
            this.consumerChannel.QueueDeclare(this.configuration.QueueName, false, false, false, null);
            this.consumerChannel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(this.consumerChannel);
            this.consumerChannel.BasicConsume(this.configuration.QueueName, false, consumer);
            consumer.Received += this.OnConsumerReceivesRequest;

            // Producer
            this.producerChannel = connection.CreateModel();
            this.producerChannel.QueueDeclare(this.configuration.ReplyQueueName, false, false);
            var producer = new EventingBasicConsumer(this.producerChannel);
            producer.Received += this.OnProducerReceivesResponse;
            this.producerChannel.BasicConsume(
                consumer: producer,
                queue: this.configuration.ReplyQueueName,
                autoAck: true);
        }

        public async Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message)
        {
            var producerProps = this.producerChannel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            producerProps.CorrelationId = correlationId;
            producerProps.ReplyTo = this.configuration.ReplyQueueName;

            var tcs = new TaskCompletionSource<byte[]>(TimeSpan.FromMilliseconds(this.configuration.ResponseTimeout));
            this.pendingMessages.TryAdd(correlationId, tcs);

            var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new RabbitMQMessage
            {
                Pattern = pattern,
                Payload = message,
                RequestType = message.GetType().FullName
            }));
            this.producerChannel.BasicPublish("", this.configuration.QueueName, producerProps, messageBytes);

            var rawResponse = await tcs.Task;
            var response = JsonSerializer.Deserialize<ServiceResponse<TPayload>>(Encoding.UTF8.GetString(rawResponse));

            return response;
        }

        private void OnProducerReceivesResponse(object sender, BasicDeliverEventArgs ea)
        {
            var correlationId = ea.BasicProperties.CorrelationId;

            // Check if it is the producer which sent the request
            if (this.pendingMessages.TryRemove(correlationId, out var tcs))
            {
                tcs.SetResult(ea.Body.ToArray());
            }
        }

        private async void OnConsumerReceivesRequest(object model, BasicDeliverEventArgs ea)
        {
            var response = default(ServiceResponse<object>);
            var body = ea.Body.ToArray();
            var props = ea.BasicProperties;
            var replyProps = this.consumerChannel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            try
            {
                var rpcMessage = JsonSerializer.Deserialize<RabbitMQMessage>(Encoding.UTF8.GetString(body));
                var service = this.serviceFactory(rpcMessage.Pattern);
                var json = ((JsonElement)rpcMessage.Payload).GetRawText();
                var request = JsonSerializer.Deserialize(json, Type.GetType(rpcMessage.RequestType));
                response = await service.Run(request);
            }
            catch (Exception e)
            {
                this.logger.LogDebug(e.ToString());
                response = new ServiceResponse<object> { Error = ErrorCode.SERVICE_NOT_FOUND };
            }
            finally
            {
                var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
                this.consumerChannel.BasicPublish("", props.ReplyTo, replyProps, responseBytes);
                this.consumerChannel.BasicAck(ea.DeliveryTag, false);
            }
        }

        public Task Shutdown()
        {
            this.connection.Close();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this.connection.Close();
        }

        class RabbitMQMessage
        {
            public string Pattern { get; init; }
            public object Payload { get; init; }
            public string RequestType { get; init; }
        }
    }

    public class RabbitMQMessageMediatorConfiguration
    {
        public string QueueName { get; init; }
        public string ReplyQueueName { get; init; }
        public string ConnectionString { get; init; }

        /// <summary>
        /// Milliseconds
        /// </summary>
        public uint ResponseTimeout { get; init; } = 10000;
    }
}