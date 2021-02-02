using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Text;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.pubsub
{
    public class RabbitMQPublisher : IPublisher
    {
        private readonly RabbitMQPublisherSettings settings;
        private readonly ConnectionFactory connectionFactory;

        public RabbitMQPublisher(RabbitMQPublisherSettings settings)
        {
            this.settings = settings;
            this.connectionFactory = new ConnectionFactory() { Uri = new Uri(this.settings.ConnectionString) };
        }

        public Task Publish(string message)
        {
            using (var connection = this.connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: this.settings.TopicName, type: ExchangeType.Fanout, durable: true);

                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: this.settings.TopicName,
                                     routingKey: "",
                                     basicProperties: null,
                                     body: body);
            }

            return Task.CompletedTask;
        }
    }

    public class RabbitMQPublisherSettings
    {
        public string ConnectionString { get; set; }
        public string TopicName { get; set; }
    }

    public class RabbitMQSubscriber : ISubscriber, IDisposable
    {
        private readonly RabbitMQSubscriberSettings settings;
        private readonly ConnectionFactory connectionFactory;

        private bool disposedValue;

        private IModel channel;
        private IConnection connection;
        private AsyncEventingBasicConsumer consumer;

        public ISubscriber.OnMessageDelegate OnMessage { get; set; }
        public ISubscriber.OnErrorDelegate OnError { get; set; }

        public RabbitMQSubscriber(RabbitMQSubscriberSettings settings)
        {
            this.settings = settings;
            this.connectionFactory = new ConnectionFactory()
            {
                Uri = new Uri(this.settings.ConnectionString),
                DispatchConsumersAsync = true
            };
        }

        public Task Subscribe()
        {
            this.connection = this.connectionFactory.CreateConnection();
            this.channel = connection.CreateModel();

            this.channel.ExchangeDeclare(exchange: this.settings.TopicName, type: ExchangeType.Fanout, durable: true);

            var queueName = this.channel.QueueDeclare(queue: this.settings.SubscriptionName, exclusive: false).QueueName;
            this.channel.QueueBind(queue: queueName,
                              exchange: this.settings.TopicName,
                              routingKey: "");

            this.consumer = new AsyncEventingBasicConsumer(channel);
            this.consumer.Received += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                this.OnMessage(message);
                if (sender is AsyncEventingBasicConsumer consumer)
                {
                    consumer.Model.BasicAck(ea.DeliveryTag, true);
                }
                await Task.Yield();
            };
            this.channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumerTag: this.settings.SubscriptionName,
                                 consumer: this.consumer);

            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.channel.Close();
                    this.connection.Close();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public sealed class RabbitMQSubscriberSettings
    {
        public string ConnectionString { get; set; }
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }
    }
}
