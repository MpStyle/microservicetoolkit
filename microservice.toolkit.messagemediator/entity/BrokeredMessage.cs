namespace microservice.toolkit.messagemediator.entity
{
    internal class BrokeredMessage
    {
        public string Pattern { get; init; }
        public object Payload { get; init; }
        public string RequestType { get; init; }
    }
}