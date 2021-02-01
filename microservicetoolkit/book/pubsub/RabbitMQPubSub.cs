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
            this.connectionFactory = new ConnectionFactory() { HostName = this.settings.ConnectionString };
        }

        public Task Publish(string message)
        {
            using (var connection = this.connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: this.settings.TopicName, type: ExchangeType.Fanout);

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

        public ISubscriber.OnMessageDelegate OnMessage { get; set; }
        public ISubscriber.OnErrorDelegate OnError { get; set; }

        public RabbitMQSubscriber(RabbitMQSubscriberSettings settings)
        {
            this.settings = settings;
            this.connectionFactory = new ConnectionFactory() { HostName = this.settings.ConnectionString };
        }

        public Task Subscribe()
        {
            this.connection = this.connectionFactory.CreateConnection();

            this.channel = connection.CreateModel();
            this.channel.ExchangeDeclare(exchange: this.settings.TopicName, type: ExchangeType.Fanout);

            var queueName = this.channel.QueueDeclare().QueueName;
            this.channel.QueueBind(queue: queueName,
                              exchange: this.settings.TopicName,
                              routingKey: "");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += this.OnMessageListener;
            this.channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);

            return Task.CompletedTask;
        }

        private async Task OnMessageListener(object sender, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            await this.OnMessage(message);
            (sender as IModel).BasicAck(ea.DeliveryTag, false);
            await Task.Yield();
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
