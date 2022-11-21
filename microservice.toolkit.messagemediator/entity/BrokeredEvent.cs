using System;

namespace microservice.toolkit.messagemediator.entity
{
    internal class BrokeredEvent
    {
        public BrokeredEvent() {
            this.Created = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public string Pattern { get; init; }
        public object Payload { get; init; }
        public string RequestType { get; init; }

        /// <summary>
        /// Timestamp in milliseconds of creation date
        /// </summary>
        public long Created { get; }
    }
}