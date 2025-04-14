using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

using microservice.toolkit.core;
using microservice.toolkit.core.entity;
using microservice.toolkit.messagemediator.entity;

using Microsoft.Extensions.Logging;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

/// <summary>
/// Represents a message mediator for sending and receiving messages using Azure Service Bus.
/// </summary>
public class ServiceBusMessageMediator : CachedMessageMediator, IDisposable
{
    private readonly ServiceFactory serviceFactory;
    private readonly ServiceBusAdministrationClient serviceBusAdministrationClient;
    private readonly Configuration configuration;
    private readonly ServiceBusClient producerClient;
    private readonly ServiceBusClient consumerClient;
    private readonly ILogger<ServiceBusMessageMediator> logger;

    public ServiceBusMessageMediator(ServiceFactory serviceFactory, Configuration configuration, ILogger<ServiceBusMessageMediator> logger)
        : this(serviceFactory, configuration, null, logger)
    {
    }
    
    public ServiceBusMessageMediator(ServiceFactory serviceFactory, Configuration configuration, ICacheManager cacheManager,
        ILogger<ServiceBusMessageMediator> logger)
        : base(cacheManager)
    {
        this.serviceFactory = serviceFactory;
        this.configuration = configuration;
        this.logger = logger;
        this.serviceBusAdministrationClient = new ServiceBusAdministrationClient(this.configuration.ConnectionString);
        this.producerClient = new ServiceBusClient(this.configuration.ConnectionString);
        this.consumerClient = new ServiceBusClient(this.configuration.ConnectionString);
    }

    public override Task Init(CancellationToken cancellationToken)
    {
        this.RegisterConsumer(cancellationToken);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends a message to a service bus queue and waits for a response.
    /// </summary>
    /// <typeparam name="TPayload">The type of the payload in the message.</typeparam>
    /// <param name="pattern">The pattern of the message.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task result contains the response from the service bus.</returns>
    public override async Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message, CancellationToken cancellationToken)
    {
        if (this.TryGetCachedResponse(pattern, message, cancellationToken, out ServiceResponse<TPayload> cachedPayload))
        {
            return cachedPayload;
        }
        
        // Temporary Queue for Receiver to send their replies into
        var replyQueueName = Guid.NewGuid().ToString();
        await this.serviceBusAdministrationClient.CreateQueueAsync(new CreateQueueOptions(replyQueueName)
        {
            AutoDeleteOnIdle = TimeSpan.FromSeconds(300)
        }, cancellationToken);

        // Sending the message
        var serviceBusSender = producerClient.CreateSender(this.configuration.QueueName);
        var applicationMessage = new BrokeredMessage
        {
            Pattern = pattern,
            Payload = message,
            RequestType = message.GetType().FullName
        };
        var serviceBusMessage = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(applicationMessage))
        {
            ContentType = "application/json",
            ReplyTo = replyQueueName,
        };

        await serviceBusSender.SendMessageAsync(serviceBusMessage);

        // Creating a receiver and waiting for the Receiver to reply
        var serviceBusReceiver = producerClient.CreateReceiver(replyQueueName);
        var serviceBusReceivedMessage = await serviceBusReceiver.ReceiveMessageAsync(TimeSpan.FromSeconds(60), cancellationToken);

        if (serviceBusReceivedMessage == null)
        {
            this.logger.LogDebug("Error: didn't receive a response");
            return new ServiceResponse<TPayload> { Error = ServiceError.ExecutionTimeout };
        }

        var response = JsonSerializer.Deserialize<ServiceResponse<TPayload>>(serviceBusReceivedMessage.Body.ToString());

        if (response == null)
        {
            return new ServiceResponse<TPayload> { Error = ServiceError.EmptyResponse };
        }
        
        this.SetCacheResponse(pattern, message, cancellationToken, response);

        return response;
    }

    /// <summary>
    /// Shuts down the service bus message mediator.
    /// </summary>
    /// <returns>A task that represents the asynchronous shutdown operation.</returns>
    public override async Task Shutdown(CancellationToken cancellationToken)
    {
        await this.producerClient.DisposeAsync();
        await this.consumerClient.DisposeAsync();
    }

    private void RegisterConsumer(CancellationToken cancellationToken)
    {
        var serviceBusProcessor = this.consumerClient.CreateProcessor(this.configuration.QueueName);

        serviceBusProcessor.ProcessMessageAsync += async args =>
        {
            var response = new ServiceResponse<object> { Error = ServiceError.EmptyRequest };
            var brokeredMessage = JsonSerializer.Deserialize<BrokeredMessage>(args.Message.Body.ToString());

            if (brokeredMessage != null)
            {
                try
                {
                    var service = this.serviceFactory(brokeredMessage.Pattern);

                    if (service == null)
                    {
                        throw new ServiceNotFoundException(brokeredMessage.Pattern);
                    }

                    var json = ((JsonElement)brokeredMessage.Payload).GetRawText();
                    var request = JsonSerializer.Deserialize(json, Type.GetType(brokeredMessage.RequestType));

                    response = await service.Run(request, cancellationToken);
                }
                catch (ServiceNotFoundException ex)
                {
                    this.logger.LogDebug("Service not found: {Message}", ex.ToString());
                    response = new ServiceResponse<object> { Error = ServiceError.ServiceNotFound };
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug("Generic error: {Message}", ex.ToString());
                    response = new ServiceResponse<object> { Error = ServiceError.Unknown };
                }
            }

            // Sending the reply
            var serviceBusSender = this.consumerClient.CreateSender(args.Message.ReplyTo);
            ServiceBusMessage serviceBusMessage = new(JsonSerializer.Serialize(response));
            await serviceBusSender.SendMessageAsync(serviceBusMessage, cancellationToken);
        };
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    public async void Dispose()
    {
        await this.Shutdown(CancellationToken.None);
    }

    /// <summary>
    /// Represents the configuration settings for the Service Bus message mediator.
    /// </summary>
    /// <param name="QueueName"> Gets or sets the name of the queue. </param>
    /// <param name="ConnectionString"> Gets or sets the connection string for the Service Bus. </param>
    public record Configuration(string QueueName, string ConnectionString);
}