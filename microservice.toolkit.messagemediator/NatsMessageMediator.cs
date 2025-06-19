using microservice.toolkit.messagemediator.entity;

using Microsoft.Extensions.Logging;

using NATS.Client;

using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator;

public class NatsMessageMediator(
    NatsMessageMediatorConfiguration configuration,
    ServiceFactory serviceFactory,
    ILogger<NatsMessageMediator> logger
) : IMessageMediator, IAsyncDisposable
{
    private IConnection connection;
    private IAsyncSubscription consumerSubscription;

    public Task InitAsync(CancellationToken cancellationToken)
    {
        this.connection = new ConnectionFactory().CreateConnection(configuration.ConnectionString);
        this.consumerSubscription = this.connection.SubscribeAsync(configuration.Topic,
            (model, ea) => _ = this.OnConsumerReceivesRequest(model, ea, cancellationToken));
        return Task.CompletedTask;
    }

    public async Task<ServiceResponse<TPayload>> SendAsync<TPayload>(
        string pattern,
        object message,
        CancellationToken cancellationToken = default
    )
    {
        var responseMessage = await this.connection.RequestAsync(configuration.Topic,
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new BrokeredEvent
            {
                Pattern = pattern,
                Payload = message,
                RequestType = message.GetType().FullName,
            })), configuration.ResponseTimeout, cancellationToken);
        var response =
            JsonSerializer.Deserialize<ServiceResponse<TPayload>>(Encoding.UTF8.GetString(responseMessage.Data));

        return response;
    }

    private async Task OnConsumerReceivesRequest(object model, MsgHandlerEventArgs ea,
        CancellationToken cancellationToken)
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
            var service = serviceFactory(rpcMessage.Pattern);

            if (service == null)
            {
                throw new MessageMediatorException(ServiceError.ServiceNotFound);
            }

            var requestType = Type.GetType(rpcMessage.RequestType);

            if (requestType == null)
            {
                throw new MessageMediatorException(ServiceError.InvalidRequestType);
            }

            var json = ((JsonElement)rpcMessage.Payload).GetRawText();
            var request = JsonSerializer.Deserialize(json, requestType);

            response = await service.RunAsync(request, cancellationToken);

            if (response == null)
            {
                throw new InvalidServiceException(service.GetType().FullName);
            }
        }
        catch (MessageMediatorException ex)
        {
            response = new ServiceResponse<object> { Error = ex.ErrorCode };
        }
        catch (Exception ex)
        {
            logger.LogDebug("Generic error: {Message}", ex.ToString());
            response = new ServiceResponse<object> { Error = ServiceError.Unknown };
        }
        finally
        {
            var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
            this.connection.Publish(ea.Message.Reply, responseBytes);
        }
    }

    public virtual async ValueTask DisposeAsync()
    {
        await this.ShutdownAsync(CancellationToken.None);
        GC.SuppressFinalize(this);
    }

    public async Task ShutdownAsync(CancellationToken cancellationToken)
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