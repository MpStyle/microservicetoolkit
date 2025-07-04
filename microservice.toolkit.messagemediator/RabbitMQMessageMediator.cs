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

namespace microservice.toolkit.messagemediator;

/// <summary>
/// Represents a message mediator for RabbitMQ.
/// </summary>
public class RabbitMQMessageMediator : IMessageMediator, IAsyncDisposable
{
    private readonly ILogger<RabbitMQMessageMediator> logger;
    private readonly RabbitMQMessageMediatorConfiguration configuration;
    private readonly ServiceFactory serviceFactory;

    private IConnection connection;
    private IChannel consumerChannel;
    private IChannel producerChannel;

    private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> pendingMessages = new();

    public RabbitMQMessageMediator(RabbitMQMessageMediatorConfiguration configuration,
        ServiceFactory serviceFactory, ILogger<RabbitMQMessageMediator> logger)
    {
        this.configuration = configuration;
        this.serviceFactory = serviceFactory;
        this.logger = logger;
    }

    public async Task Init(CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory() { HostName = this.configuration.ConnectionString };
        this.connection = await factory.CreateConnectionAsync(cancellationToken);

        // Consumer
        this.consumerChannel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await this.consumerChannel.QueueDeclareAsync(this.configuration.QueueName, false, false, false, null,
            cancellationToken: cancellationToken);
        await this.consumerChannel.BasicQosAsync(0, 1, false, cancellationToken);
        var consumer = new AsyncEventingBasicConsumer(this.consumerChannel);
        await this.consumerChannel.BasicConsumeAsync(this.configuration.QueueName, false, consumer,
            cancellationToken: cancellationToken);
        consumer.ReceivedAsync += this.OnConsumerReceivesRequest;

        // Producer
        this.producerChannel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await this.producerChannel.QueueDeclareAsync(this.configuration.ReplyQueueName, false, false,
            cancellationToken: cancellationToken);
        var producer = new AsyncEventingBasicConsumer(this.producerChannel);
        producer.ReceivedAsync += this.OnProducerReceivesResponse;
        await this.producerChannel.BasicConsumeAsync(
            consumer: producer,
            queue: this.configuration.ReplyQueueName,
            autoAck: true, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Sends a message to the specified pattern using the RabbitMQ message mediator.
    /// </summary>
    /// <typeparam name="TPayload">The type of the payload in the message.</typeparam>
    /// <param name="pattern">The pattern to send the message to.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation. The task result contains the response from the message mediator.</returns>
    public async Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException("Pattern must not be null or empty.", nameof(pattern));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var response = await this.InternalSendAsync<TPayload>(
                new BrokeredMessage {Pattern = pattern, Payload = message, RequestType = message.GetType().FullName,},
                cancellationToken);

            return response;
        }
        catch (ArgumentNullException ex)
        {
            logger.LogError(ex, "Argument null: {Message}", ex.Message);
            return new ServiceResponse<TPayload>
            {
                Error = ServiceError.NullRequest
            };
        } 
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Invalid argument: {Message}", ex.Message);
            return new ServiceResponse<TPayload>
            {
                Error = ServiceError.InvalidPattern    
            };
        }
    }

    /// <summary>
    /// Sends a brokered message and waits for the response.
    /// </summary>
    /// <typeparam name="TPayload">The type of the payload in the response.</typeparam>
    /// <param name="message">The brokered message to send.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task result contains the response as a <see cref="ServiceResponse{TPayload}"/>.</returns>
    private async Task<ServiceResponse<TPayload>> InternalSendAsync<TPayload>(BrokeredMessage message,
    CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        var producerProps = new BasicProperties
        {
            CorrelationId = correlationId,
            ReplyTo = this.configuration.ReplyQueueName
        };
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(this.configuration.ResponseTimeout);

        this.pendingMessages.TryAdd(correlationId, tcs);

        try
        {
            await this.producerChannel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: this.configuration.QueueName,
                mandatory: true,
                basicProperties: producerProps,
                body: messageBytes,
                cancellationToken: cts.Token);

            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(this.configuration.ResponseTimeout, cts.Token));
            if (completedTask != tcs.Task)
            {
                this.pendingMessages.TryRemove(correlationId, out _);
                return new ServiceResponse<TPayload> {Error = ServiceError.Timeout};
            }

            var rawResponse = await tcs.Task;

            if (rawResponse == null || rawResponse.Length == 0)
            {
                throw new InvalidServiceException(message.Pattern);
            }

            var response = JsonSerializer.Deserialize<ServiceResponse<TPayload?>>(Encoding.UTF8.GetString(rawResponse));
            return response;
        }
        catch (InvalidServiceException ex)
        {
            logger.LogError(ex, "Invalid service: {Message}", ex.Message);
            return new ServiceResponse<TPayload> {Error = ServiceError.NullResponse};
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "JSON deserialization error: {Message}", ex.Message);
            return new ServiceResponse<TPayload> { Error = ServiceError.ResponseDeserializationError };
        }
        catch (Exception ex)
        {
            this.logger.LogDebug("Time out error: {Message}", ex.ToString());
            return new ServiceResponse<TPayload> { Error = ServiceError.Timeout };
        }
        finally
        {
            this.pendingMessages.TryRemove(correlationId, out _);
        }
    }

    private Task OnProducerReceivesResponse(object sender, BasicDeliverEventArgs ea)
    {
        var correlationId = ea.BasicProperties.CorrelationId;
        if (this.pendingMessages.TryRemove(correlationId, out var tcs))
        {
            tcs.SetResult(ea.Body.ToArray());
        }
        return Task.CompletedTask;
    }

    private async Task OnConsumerReceivesRequest(object model, BasicDeliverEventArgs ea)
    {
        var response = default(ServiceResponse<object>);
        var body = ea.Body.ToArray();
        var props = ea.BasicProperties;
        var replyProps = new BasicProperties { CorrelationId = props.CorrelationId, };

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

            var requestType = Type.GetType(rpcMessage.RequestType);

            if (requestType == null)
            {
                throw new RabbitMQMessageMediatorException(ServiceError.InvalidRequestType);
            }
            
            var request=((JsonElement)rpcMessage.Payload).Deserialize(requestType);
            response = await service.RunAsync(request, CancellationToken.None);

            if (response == null)
            {
                throw new InvalidServiceException(service.GetType().FullName);
            }
        }
        catch (RabbitMQMessageMediatorException ex)
        {
            this.logger.LogError(ex, "RabbitMQ message mediator error: {Message}", ex.Message);
            response = new ServiceResponse<object> {Error = ex.ErrorCode};
        }
        catch (JsonException ex)
        {
            this.logger.LogError(ex, "JSON deserialization error: {Message}", ex.Message);
            response = new ServiceResponse<object> { Error = ServiceError.RequestDeserializationError };
        }
        catch (Exception ex)
        {
            this.logger.LogError("Generic error: {Message}", ex.ToString());
            response = new ServiceResponse<object> { Error = ServiceError.Unknown };
        }
        finally
        {
            var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
            await this.consumerChannel.BasicPublishAsync(exchange: string.Empty, routingKey: props.ReplyTo,
                mandatory: true, basicProperties: replyProps, body: responseBytes);
            await this.consumerChannel.BasicAckAsync(ea.DeliveryTag, false);
        }
    }

    public virtual async ValueTask DisposeAsync()
    {
        await this.Shutdown(CancellationToken.None);
        GC.SuppressFinalize(this);
    }

    public async Task Shutdown(CancellationToken cancellationToken)
    {
        if (this.consumerChannel != null)
        {
            await this.consumerChannel.CloseAsync(cancellationToken);
        }

        if (this.producerChannel != null)
        {
            await this.producerChannel.CloseAsync(cancellationToken);
        }

        if (this.connection != null)
        {
            await this.connection.CloseAsync(cancellationToken);
        }
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
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the RabbitMQMessageMediatorException class with the specified error code.
    /// </summary>
    /// <param name="errorCode">The error code associated with the exception.</param>
    public RabbitMQMessageMediatorException(string errorCode)
    {
        this.ErrorCode = errorCode;
    }
}