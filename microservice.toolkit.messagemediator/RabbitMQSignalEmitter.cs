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
public class RabbitMQSignalEmitter : ISignalEmitter, IAsyncDisposable
{
    private readonly ILogger<RabbitMQSignalEmitter> logger;
    private readonly RabbitMQSignalEmitterConfiguration configuration;
    private readonly SignalHandlerFactory serviceFactory;

    private IConnection connection;
    private IChannel consumerChannel;
    private IChannel producerChannel;
    private readonly object producerLock = new();

    public RabbitMQSignalEmitter(
        RabbitMQSignalEmitterConfiguration configuration,
        SignalHandlerFactory serviceFactory,
        ILogger<RabbitMQSignalEmitter> logger)
    {
        this.configuration = configuration;
        this.serviceFactory = serviceFactory;
        this.logger = logger;
    }

    public async Task Init(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory() {HostName = this.configuration.ConnectionString};
        this.connection = await factory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);

        // Consumer
        this.consumerChannel =
            await connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        await this.consumerChannel.QueueDeclareAsync(
            this.configuration.QueueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        await this.consumerChannel.BasicQosAsync(0, 1, false, cancellationToken).ConfigureAwait(false);

        var consumer = new AsyncEventingBasicConsumer(this.consumerChannel);
        await this.consumerChannel.BasicConsumeAsync(
            this.configuration.QueueName,
            autoAck: false,
            consumer,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        consumer.ReceivedAsync += (model, ea) => this.OnConsumerReceivesRequest(model, ea, cancellationToken);

        // Producer
        this.producerChannel =
            await connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public Task Emit<TEvent>(string pattern, TEvent myEvent, CancellationToken cancellationToken)
    {
        if (producerChannel == null)
        {
            throw new InvalidOperationException("Producer channel is not initialized.");
        }

        var brokeredEvent = new BrokeredEvent
        {
            Pattern = pattern, Payload = myEvent, RequestType = myEvent?.GetType().FullName,
        };
        var eventBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(brokeredEvent));

        // Thread safety for producer channel
        lock (producerLock)
        {
            return producerChannel.BasicPublishAsync(
                exchange: "",
                routingKey: this.configuration.QueueName,
                body: eventBytes,
                cancellationToken: cancellationToken).AsTask();
        }
    }

    public Task EmitAsync<TEvent>(string pattern, TEvent myEvent)
    {
        // Fire-and-forget, log exceptions if any
        _ = Task.Run(async () =>
        {
            try
            {
                await this.Emit(pattern, myEvent, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in EmitAsync");
            }
        });
        return Task.CompletedTask;
    }

    private async Task OnConsumerReceivesRequest(object model, BasicDeliverEventArgs ea,
        CancellationToken cancellationToken)
    {
        var body = ea.Body.ToArray();
        BrokeredEvent brokeredEvent = null;
        try
        {
            brokeredEvent = JsonSerializer.Deserialize<BrokeredEvent>(Encoding.UTF8.GetString(body));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize BrokeredEvent");
        }

        if (brokeredEvent == null)
        {
            logger.LogWarning("Received null or invalid BrokeredEvent from queue.");
            return;
        }

        try
        {
            var requestType = Type.GetType(brokeredEvent.RequestType);
            if (requestType == null)
            {
                throw new SignalEmitterException(ServiceError.InvalidRequestType);
            }

            var eventHandlers = this.serviceFactory(brokeredEvent.Pattern);
            var json = ((JsonElement)brokeredEvent.Payload).GetRawText();
            var request = JsonSerializer.Deserialize(json, requestType);

            foreach (var eventHandler in eventHandlers)
            {
                _ = eventHandler.Run(request, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling received event");
        }
    }

    public virtual async ValueTask DisposeAsync()
    {
        await this.Shutdown(CancellationToken.None).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    public async Task Shutdown(CancellationToken cancellationToken)
    {
        if (producerChannel != null)
        {
            await producerChannel.CloseAsync(cancellationToken).ConfigureAwait(false);
            await producerChannel.DisposeAsync().ConfigureAwait(false);
            producerChannel = null;
        }

        if (consumerChannel != null)
        {
            await consumerChannel.CloseAsync(cancellationToken).ConfigureAwait(false);
            await consumerChannel.DisposeAsync().ConfigureAwait(false);
            consumerChannel = null;
        }

        if (connection != null)
        {
            await connection.CloseAsync(cancellationToken).ConfigureAwait(false);
            await connection.DisposeAsync().ConfigureAwait(false);
            connection = null;
        }
    }
}

/// <summary>
/// Represents the configuration for a RabbitMQ signal emitter.
/// </summary>
public record RabbitMQSignalEmitterConfiguration(string QueueName, string ConnectionString);