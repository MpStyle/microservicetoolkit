using microservice.toolkit.core;
using microservice.toolkit.core.entity;
using microservice.toolkit.messagemediator.entity;

using Microsoft.Extensions.Logging;

using NATS.Client;

using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

public class NatsMessageMediator : CachedMessageMediator, IDisposable
{
    private readonly ILogger<NatsMessageMediator> logger;
    private readonly NatsMessageMediatorConfiguration configuration;
    private readonly ServiceFactory serviceFactory;
    private readonly IConnection connection;
    private readonly IAsyncSubscription consumerSubscription;

    public NatsMessageMediator(NatsMessageMediatorConfiguration configuration, ServiceFactory serviceFactory, ILogger<NatsMessageMediator> logger)
        : this(configuration, serviceFactory, null, logger)
    {
    }
    
    public NatsMessageMediator(NatsMessageMediatorConfiguration configuration, ServiceFactory serviceFactory, ICacheManager cacheManager, ILogger<NatsMessageMediator> logger)
        : base(cacheManager)
    {
        this.configuration = configuration;
        this.serviceFactory = serviceFactory;
        this.logger = logger;
        this.connection = new ConnectionFactory().CreateConnection(this.configuration.ConnectionString);
        this.consumerSubscription = this.connection.SubscribeAsync(this.configuration.Topic, this.OnConsumerReceivesRequest);
    }

    public override async Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message)
    {
        if (this.TryGetCachedResponse(pattern, message, out ServiceResponse<TPayload> cachedPayload))
        {
            return cachedPayload;
        }
        
        var responseMessage = await this.connection.RequestAsync(this.configuration.Topic, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new BrokeredEvent
        {
            Pattern = pattern,
            Payload = message,
            RequestType = message.GetType().FullName
        })), this.configuration.ResponseTimeout);
        var response = JsonSerializer.Deserialize<ServiceResponse<TPayload>>(Encoding.UTF8.GetString(responseMessage.Data));

        this.SetCacheResponse(pattern, message, response);
        
        return response;
    }

    private async void OnConsumerReceivesRequest(object model, MsgHandlerEventArgs ea)
    {
        var response = default(ServiceResponse<object>);
        var body = ea.Message.Data;
        var rpcMessage = JsonSerializer.Deserialize<BrokeredMessage>(Encoding.UTF8.GetString(body));

        // Invalid message from topic
        if (rpcMessage == null)
        {
            return;
        }

        try
        {
            var service = this.serviceFactory(rpcMessage.Pattern);

            if (service == null)
            {
                throw new NatsMessageMediatorException(ServiceError.ServiceNotFound);
            }

            var json = ((JsonElement)rpcMessage.Payload).GetRawText();
            var request = JsonSerializer.Deserialize(json, Type.GetType(rpcMessage.RequestType));
            response = await service.Run(request);
        }
        catch (NatsMessageMediatorException ex)
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
            this.connection.Publish(ea.Message.Reply, responseBytes);
        }
    }

    public void Dispose()
    {
        this.Shutdown().Wait();
    }

    public override async Task Shutdown()
    {
        if (this.consumerSubscription != null)
        {
            await this.consumerSubscription.DrainAsync();
        }

        if (this.connection != null)
        {
            await this.connection.DrainAsync();
            this.connection.Close();
        }
    }
}

public class NatsMessageMediatorConfiguration
{
    public string Topic { get; init; }
    public string ConnectionString { get; init; }

    /// <summary>
    /// Milliseconds
    /// </summary>
    public int ResponseTimeout { get; init; } = 5000;
}

public class NatsMessageMediatorException : Exception
{
    public int ErrorCode { get; }

    public NatsMessageMediatorException(int errorCode)
    {
        this.ErrorCode = errorCode;
    }
}