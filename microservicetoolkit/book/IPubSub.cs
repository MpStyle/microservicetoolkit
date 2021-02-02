using System;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book
{
    public interface IPublisher
    {
        public Task Publish(string message);
    }

    public interface ISubscriber
    {
        delegate void OnMessageDelegate(string message);
        delegate void OnErrorDelegate(Exception ex);

        public OnMessageDelegate OnMessage { get; set; }
        public OnErrorDelegate OnError { get; set; }

        public Task Subscribe();
    }
}
