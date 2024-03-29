using microservice.toolkit.core;
using microservice.toolkit.messagemediator.entity;

using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator
{
    /// <summary>
    /// Represents a signal emitter for RabbitMQ.
    /// </summary>
    public class RabbitMQSignalEmitter : ISignalEmitter, IDisposable
    {
        private readonly ILogger<RabbitMQSignalEmitter> logger;
        private readonly RabbitMQSignalEmitterConfiguration configuration;
        private readonly SignalHandlerFactory serviceFactory;

        private readonly IConnection connection;
        private readonly IModel consumerChannel;
        private readonly IModel producerChannel;

        public RabbitMQSignalEmitter(RabbitMQSignalEmitterConfiguration configuration,
            SignalHandlerFactory serviceFactory, ILogger<RabbitMQSignalEmitter> logger)
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
        }

        /// <summary>
        /// Emits a message to a RabbitMQ exchange.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event being emitted.</typeparam>
        /// <param name="pattern">The pattern for routing the message.</param>
        /// <param name="myEvent">The event to be emitted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Emit<TEvent>(string pattern, TEvent myEvent)
        {
            var brokeredEvent = new BrokeredEvent
            {
                Pattern = pattern,
                Payload = myEvent,
                RequestType = myEvent.GetType().FullName,
            };
            var producerProps = this.producerChannel.CreateBasicProperties();
            var eventBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(brokeredEvent));

            this.producerChannel.BasicPublish("", this.configuration.QueueName, producerProps, eventBytes);
            return Task.CompletedTask;
        }

        private void OnConsumerReceivesRequest(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var brokeredEvent = JsonSerializer.Deserialize<BrokeredEvent>(Encoding.UTF8.GetString(body));

            // Invalid event from queue
            if (brokeredEvent == null)
            {
                return;
            }

            try
            {
                var eventHandlers = this.serviceFactory(brokeredEvent.Pattern);
                var json = ((JsonElement)brokeredEvent.Payload).GetRawText();
                var request = JsonSerializer.Deserialize(json, Type.GetType(brokeredEvent.RequestType));

                foreach (var eventHandler in eventHandlers)
                {
                    _ = eventHandler.Run(request).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogDebug("Generic error: {Message}", ex.ToString());
            }
        }

        public void Dispose()
        {
            this.Shutdown();
        }

        public Task Shutdown()
        {
            this.connection.Close();
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Represents the configuration for a RabbitMQ signal emitter.
    /// </summary>
    public record RabbitMQSignalEmitterConfiguration(string QueueName, string ConnectionString);

}