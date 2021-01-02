namespace mpstyle.microservice.toolkit.book
{
    public static class SettingKey
    {
        public static class Cache
        {
            public const string SERVERS = "mt:cache:servers";
        }

        public static class Database
        {
            public const string CONNECTION_STRING = "mt:database:connectionString";
        }

        public static class Rpc
        {
            public const string QUEUE_NAME = "mt:rpc:queueName";
            public const string CONNECTION_STRING = "mt:rpc:connectionstring";
            public const string CONSUMERS_PER_QUEUE = "mt:rpc:consumersPerQueue";
            public const string REPLY_QUEUE_NAME = "mt:rpc:replyQueueName";
        }

        public static class ServiceBus
        {
            public const string QUEUE_NAME = "mt:servicebus:queueName";
            public const string CONNECTION_STRING = "mt:servicebus:connectionstring";
            public const string CONSUMERS_PER_QUEUE = "mt:servicebus:consumersPerQueue";
            public const string REPLY_QUEUE_NAME = "mt:servicebus:replyQueueName";
        }

        public static class Migration
        {
            public const string EXTENSION = "mt:migration:extension";
        }
    }
}
