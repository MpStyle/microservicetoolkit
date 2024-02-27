using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

using microservice.toolkit.core;
using microservice.toolkit.core.entity;
using microservice.toolkit.messagemediator.entity;

using Microsoft.Extensions.Logging;

using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

public class ServiceBusMessageMediator : IMessageMediator, IDisposable
{
    private readonly ServiceFactory serviceFactory;
    private readonly ServiceBusAdministrationClient serviceBusAdministrationClient;
    private readonly Configuration configuration;
    private readonly ServiceBusClient producerClient;
    private readonly ServiceBusClient consumerClient;
    private readonly ILogger<ServiceBusMessageMediator> logger;

    public ServiceBusMessageMediator(ServiceFactory serviceFactory, Configuration configuration,
        ILogger<ServiceBusMessageMediator> logger)
    {
        this.serviceFactory = serviceFactory;
        this.configuration = configuration;
        this.logger = logger;
        this.serviceBusAdministrationClient = new ServiceBusAdministrationClient(this.configuration.ConnectionString);
        this.producerClient = new ServiceBusClient(this.configuration.ConnectionString);
        this.consumerClient = new ServiceBusClient(this.configuration.ConnectionString);

        this.RegisterConsumer();
    }

    public async Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message)
    {
        // Temporary Queue for Receiver to send their replies into
        var replyQueueName = Guid.NewGuid().ToString();
        await this.serviceBusAdministrationClient.CreateQueueAsync(new CreateQueueOptions(replyQueueName)
        {
            AutoDeleteOnIdle = TimeSpan.FromSeconds(300)
        });

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
            ContentType = "application/json", ReplyTo = replyQueueName,
        };

        await serviceBusSender.SendMessageAsync(serviceBusMessage);

        // Creating a receiver and waiting for the Receiver to reply
        var serviceBusReceiver = producerClient.CreateReceiver(replyQueueName);
        var serviceBusReceivedMessage = await serviceBusReceiver.ReceiveMessageAsync(TimeSpan.FromSeconds(60));

        if (serviceBusReceivedMessage == null)
        {
            this.logger.LogDebug("Error: didn't receive a response");
            return new ServiceResponse<TPayload> { Error = ErrorCode.ExecutionTimeout };
        }

        var response = JsonSerializer.Deserialize<ServiceResponse<TPayload>>(serviceBusReceivedMessage.Body.ToString());

        if (response == null)
        {
            return new ServiceResponse<TPayload> { Error = ErrorCode.EmptyResponse };
        }

        return response;
    }

    public async Task Shutdown()
    {
        await this.producerClient.DisposeAsync();
        await this.consumerClient.DisposeAsync();
    }

    private void RegisterConsumer()
    {
        var serviceBusProcessor = this.consumerClient.CreateProcessor(this.configuration.QueueName);

        serviceBusProcessor.ProcessMessageAsync += async args =>
        {
            var response = new ServiceResponse<object> { Error = ErrorCode.EmptyRequest };
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

                    response = await service.Run(request);
                }
                catch (ServiceNotFoundException ex)
                {
                    this.logger.LogDebug("Service not found: {Message}", ex.ToString());
                    response = new ServiceResponse<object> { Error = ErrorCode.ServiceNotFound };
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug("Generic error: {Message}", ex.ToString());
                    response = new ServiceResponse<object> { Error = ErrorCode.Unknown };
                }
            }

            // Sending the reply
            var serviceBusSender = this.consumerClient.CreateSender(args.Message.ReplyTo);
            ServiceBusMessage serviceBusMessage = new(JsonSerializer.Serialize(response));
            await serviceBusSender.SendMessageAsync(serviceBusMessage);
        };
    }

    public async void Dispose()
    {
        await this.Shutdown();
    }

    public class Configuration
    {
        public string QueueName { get; init; }
        public string ConnectionString { get; init; }
    }
}