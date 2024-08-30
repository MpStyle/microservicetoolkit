using microservice.toolkit.core;
using microservice.toolkit.core.entity;
using microservice.toolkit.core.extension;
using microservice.toolkit.messagemediator.entity;

using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator
{
    /// <summary>
    /// Represents a message mediator for RabbitMQ.
    /// </summary>
    public class RabbitMQMessageMediator : CachedMessageMediator, IDisposable
    {
        private readonly ILogger<RabbitMQMessageMediator> logger;
        private readonly RabbitMQMessageMediatorConfiguration configuration;
        private readonly ServiceFactory serviceFactory;

        private readonly IConnection connection;
        private readonly IModel consumerChannel;
        private readonly IModel producerChannel;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> pendingMessages = new();

        public RabbitMQMessageMediator(RabbitMQMessageMediatorConfiguration configuration, ServiceFactory serviceFactory, ILogger<RabbitMQMessageMediator> logger)
            : this(configuration, serviceFactory, null, logger)
        {
        }
        
        public RabbitMQMessageMediator(RabbitMQMessageMediatorConfiguration configuration,
            ServiceFactory serviceFactory, ICacheManager cacheManager, ILogger<RabbitMQMessageMediator> logger)
            : base(cacheManager)
        {
            this.configuration = configuration;
            this.serviceFactory = serviceFactory;
            this.logger = logger;

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

        /// <summary>
        /// Sends a message to the specified pattern using the RabbitMQ message mediator.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload in the message.</typeparam>
        /// <param name="pattern">The pattern to send the message to.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the response from the message mediator.</returns>
        public override async Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message)
        {
            if (this.TryGetCachedResponse(pattern, message, out ServiceResponse<TPayload> cachedPayload))
            {
                return cachedPayload;
            }
            
            var response= await this.Send<TPayload>(new BrokeredMessage
            {
                Pattern = pattern,
                Payload = message,
                RequestType = message.GetType().FullName,
            });
            
            this.SetCacheResponse(pattern, message, response);

            return response;
        }

        /// <summary>
        /// Sends a brokered message and waits for the response.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload in the response.</typeparam>
        /// <param name="message">The brokered message to send.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task result contains the response as a <see cref="ServiceResponse{TPayload}"/>.</returns>
        private async Task<ServiceResponse<TPayload>> Send<TPayload>(BrokeredMessage message)
        {
            var correlationId = Guid.NewGuid().ToString();

            var producerProps = this.producerChannel.CreateBasicProperties();
            producerProps.CorrelationId = correlationId;
            producerProps.ReplyTo = this.configuration.ReplyQueueName;

            var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            try
            {
                var tcs = new TaskCompletionSource<byte[]>();

                var ct = new CancellationTokenSource(this.configuration.ResponseTimeout);
                ct.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

                this.pendingMessages.TryAdd(correlationId, tcs);

                this.producerChannel.BasicPublish("", this.configuration.QueueName, producerProps, messageBytes);

                var rawResponse = await tcs.Task;
                var response = JsonSerializer.Deserialize<ServiceResponse<TPayload>>(Encoding.UTF8.GetString(rawResponse));

                return response;
            }
            catch (Exception ex)
            {
                this.logger.LogDebug("Time out error: {Message}", ex.ToString());
                return new ServiceResponse<TPayload>
                {
                    Error = ServiceError.TimeOut
                };
            }
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

            var rpcMessage = JsonSerializer.Deserialize<BrokeredMessage>(Encoding.UTF8.GetString(body));

            // Invalid message from the queue or invalid props
            if (rpcMessage == null || props.ReplyTo.IsNullOrEmpty() || props.CorrelationId.IsNullOrEmpty())
            {
                return;
            }

            try
            {
                var service = this.serviceFactory(rpcMessage.Pattern);

                if (service == null)
                {
                    throw new RabbitMQMessageMediatorException(ServiceError.ServiceNotFound);
                }

                var json = ((JsonElement)rpcMessage.Payload).GetRawText();
                var request = JsonSerializer.Deserialize(json, Type.GetType(rpcMessage.RequestType));
                response = await service.Run(request);
            }
            catch (RabbitMQMessageMediatorException ex)
            {
                response = new ServiceResponse<object> { Error = ex.ErrorCode };
            }
            catch (Exception ex)
            {
                this.logger.LogDebug("Generic error: {Message}", ex.ToString());
                response = new ServiceResponse<object> { Error = ServiceError.Unknown };
            }
            finally
            {
                var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
                this.consumerChannel.BasicPublish("", props.ReplyTo, replyProps, responseBytes);
                this.consumerChannel.BasicAck(ea.DeliveryTag, false);
            }
        }

        public void Dispose()
        {
            this.Shutdown();
        }

        public override Task Shutdown()
        {
            this.connection.Close();
            return Task.CompletedTask;
        }
    }



    /// <summary>
    /// Represents the configuration for the RabbitMQ message mediator.
    /// </summary>
    /// <param name="QueueName"> Gets or sets the name of the queue. </param>
    /// <param name="ReplyQueueName"> Gets or sets the name of the reply queue. </param>
    /// <param name="ConnectionString"> Gets or sets the connection string for RabbitMQ. </param>
    public record RabbitMQMessageMediatorConfiguration(string QueueName, string ReplyQueueName, string ConnectionString)
    {
        /// <summary>
        /// Gets or sets the response timeout in milliseconds.
        /// </summary>
        /// <remarks>
        /// The default value is 10000 milliseconds (10 seconds).
        /// </remarks>
        public int ResponseTimeout { get; init; } = 10000;
    }

    /// <summary>
    /// Represents an exception that is thrown by the RabbitMQMessageMediator class.
    /// </summary>
    public class RabbitMQMessageMediatorException : Exception
    {
        /// <summary>
        /// Gets the error code associated with the exception.
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Initializes a new instance of the RabbitMQMessageMediatorException class with the specified error code.
        /// </summary>
        /// <param name="errorCode">The error code associated with the exception.</param>
        public RabbitMQMessageMediatorException(int errorCode)
        {
            this.ErrorCode = errorCode;
        }
    }
}