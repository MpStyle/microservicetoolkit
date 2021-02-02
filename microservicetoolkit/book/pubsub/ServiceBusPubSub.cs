using Azure.Messaging.ServiceBus;

using Microsoft.Azure.ServiceBus.Management;

using System;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.pubsub
{
    public class ServiceBusPublisher : IPublisher
    {
        private readonly ServiceBusPublisherSettings settings;

        public ServiceBusPublisher(ServiceBusPublisherSettings settings)
        {
            this.settings = settings;
        }

        public async Task Publish(string message)
        {
            await using (var client = new ServiceBusClient(this.settings.ConnectionString))
            {
                var sender = client.CreateSender(this.settings.TopicName);
                await sender.SendMessageAsync(new ServiceBusMessage(message));
            }
        }
    }

    public sealed class ServiceBusPublisherSettings
    {
        public string ConnectionString { get; set; }
        public string TopicName { get; set; }
    }

    public class ServiceBusSubscriber : ISubscriber, IAsyncDisposable
    {
        private readonly ServiceBusSubscriberSettings settings;

        private bool disposedValue;
        private ServiceBusClient client;
        private ServiceBusProcessor processor;

        public ISubscriber.OnMessageDelegate OnMessage { get; set; }
        public ISubscriber.OnErrorDelegate OnError { get; set; }

        public ServiceBusSubscriber(ServiceBusSubscriberSettings settings)
        {
            this.settings = settings;
        }

        public async Task Subscribe()
        {
            var managementClient = new ManagementClient(this.settings.ConnectionString);
            if (!await managementClient.SubscriptionExistsAsync(this.settings.TopicName, this.settings.SubscriptionName))
            {
                await managementClient.CreateSubscriptionAsync(new SubscriptionDescription(this.settings.TopicName, this.settings.SubscriptionName));
            }

            this.client = new ServiceBusClient(this.settings.ConnectionString);
            this.processor = client.CreateProcessor(this.settings.TopicName, this.settings.SubscriptionName, new ServiceBusProcessorOptions());

            processor.ProcessMessageAsync += this.OnMessageListener;
            processor.ProcessErrorAsync += this.OnErrorListener;

            await processor.StartProcessingAsync();
        }

        protected virtual async Task Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    processor.ProcessMessageAsync -= this.OnMessageListener;
                    processor.ProcessErrorAsync -= this.OnErrorListener;

                    await this.processor.StopProcessingAsync();
                    await this.client.DisposeAsync();
                }

                disposedValue = true;
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private Task OnMessageListener(ProcessMessageEventArgs args)
        {
            this.OnMessage(args.Message.Body.ToString());
            return Task.CompletedTask;
        }

        private Task OnErrorListener(ProcessErrorEventArgs args)
        {
            this.OnError(args.Exception);
            return Task.CompletedTask;
        }
    }

    public sealed class ServiceBusSubscriberSettings
    {
        public string ConnectionString { get; set; }
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }
    }
}
