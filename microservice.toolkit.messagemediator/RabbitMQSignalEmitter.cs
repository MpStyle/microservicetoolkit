using microservice.toolkit.core;
using microservice.toolkit.messagemediator.entity;

using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

/// <summary>
/// Represents a signal emitter for RabbitMQ.
/// </summary>
public class RabbitMQSignalEmitter : ISignalEmitter, IDisposable
{
    private readonly ILogger<RabbitMQSignalEmitter> logger;
    private readonly RabbitMQSignalEmitterConfiguration configuration;
    private readonly SignalHandlerFactory serviceFactory;

    private IConnection connection;
    private IChannel consumerChannel;
    private IChannel producerChannel;

    public RabbitMQSignalEmitter(RabbitMQSignalEmitterConfiguration configuration,
        SignalHandlerFactory serviceFactory, ILogger<RabbitMQSignalEmitter> logger)
    {
        this.configuration = configuration;
        this.serviceFactory = serviceFactory;
        this.logger = logger;
    }

    public async Task Init(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory() {HostName = this.configuration.ConnectionString};
        this.connection = await factory.CreateConnectionAsync(cancellationToken);

        // Consumer
        this.consumerChannel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await this.consumerChannel.QueueDeclareAsync(this.configuration.QueueName, false, false, false, null, cancellationToken: cancellationToken);
        await this.consumerChannel.BasicQosAsync(0, 1, false, cancellationToken);
        var consumer = new AsyncEventingBasicConsumer(this.consumerChannel);
        await this.consumerChannel.BasicConsumeAsync(this.configuration.QueueName, false, consumer, cancellationToken: cancellationToken);
        consumer.ReceivedAsync += (model, ea) 
            => this.OnConsumerReceivesRequest(model, ea, cancellationToken);

        // Producer
        this.producerChannel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Emits a message to a RabbitMQ exchange.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event being emitted.</typeparam>
    /// <param name="pattern">The pattern for routing the message.</param>
    /// <param name="myEvent">The event to be emitted.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Emit<TEvent>(string pattern, TEvent myEvent, CancellationToken cancellationToken)
    {
        var brokeredEvent = new BrokeredEvent
        {
            Pattern = pattern, Payload = myEvent, RequestType = myEvent.GetType().FullName,
        };
        var eventBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(brokeredEvent));

        await this.producerChannel.BasicPublishAsync("", this.configuration.QueueName, eventBytes,
            cancellationToken: cancellationToken);
    }

    public async Task Emit<TEvent>(string pattern, TEvent myEvent)
    {
        _ = this.Emit(pattern, myEvent, CancellationToken.None);
    }

    private async Task OnConsumerReceivesRequest(object model, BasicDeliverEventArgs ea,
        CancellationToken cancellationToken)
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
                _ = eventHandler.Run(request, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogDebug("Generic error: {Message}", ex.ToString());
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _ = this.Shutdown(CancellationToken.None);
    }

    public async Task Shutdown(CancellationToken cancellationToken)
    {
        await this.connection.CloseAsync(cancellationToken: cancellationToken);
    }
}

/// <summary>
/// Represents the configuration for a RabbitMQ signal emitter.
/// </summary>
public record RabbitMQSignalEmitterConfiguration(string QueueName, string ConnectionString);