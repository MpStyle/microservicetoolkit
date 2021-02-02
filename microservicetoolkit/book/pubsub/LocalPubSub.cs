using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.pubsub
{
    public class LocalPublisher : IPublisher
    {
        private readonly LocalPublisherSettings settings;

        public LocalPublisher(LocalPublisherSettings settings)
        {
            this.settings = settings;
        }

        public Task Publish(string message)
        {
            return LocalMessageBroker.GetInstance().Publish(this.settings.TopicName, message);
        }
    }

    public class LocalPublisherSettings
    {
        public string TopicName { get; set; }
    }

    public class LocalSubscriber : ISubscriber
    {
        private readonly LocalSubscriberSettings settings;

        public ISubscriber.OnMessageDelegate OnMessage { get; set; }
        public ISubscriber.OnErrorDelegate OnError { get; set; }
        public string SubscriptionName { get => this.settings.SubscriptionName; }

        public LocalSubscriber(LocalSubscriberSettings settings)
        {
            this.settings = settings;
        }

        public Task Subscribe()
        {
            LocalMessageBroker.GetInstance().Subscribe(this.settings.TopicName, this);
            return Task.CompletedTask;
        }

        internal void OnMessageReceived(string message)
        {
            this.OnMessage(message);
        }
    }

    public class LocalSubscriberSettings
    {
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }
    }

    internal class LocalMessageBroker
    {
        private readonly Dictionary<string, List<LocalSubscriber>> subscribers = new Dictionary<string, List<LocalSubscriber>>();
        private static LocalMessageBroker messageBroker;

        private LocalMessageBroker() { }

        internal static LocalMessageBroker GetInstance()
        {
            if (messageBroker == null)
            {
                messageBroker = new LocalMessageBroker();
            }

            return messageBroker;
        }

        internal Task Publish(string topic, string message)
        {
            if (this.subscribers.ContainsKey(topic))
            {
                var task = new Task(() =>
                {
                    this.subscribers[topic].ForEach(s => s.OnMessageReceived(message));
                });

                task.Start();

                return task;
            }

            return Task.CompletedTask;
        }

        internal void Subscribe(string topic, LocalSubscriber subscriber)
        {
            if (this.subscribers.ContainsKey(topic) == false)
            {
                this.subscribers[topic] = new List<LocalSubscriber>();
            }

            if (this.subscribers[topic].Contains(subscriber) == false && this.subscribers[topic].Any(s => s.SubscriptionName == subscriber.SubscriptionName) == false)
            {
                this.subscribers[topic].Add(subscriber);
            }
        }
    }
}
