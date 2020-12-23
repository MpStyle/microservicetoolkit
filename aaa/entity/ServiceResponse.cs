using System.Text.Json.Serialization;

namespace mpstyle.microservice.toolkit.entity
{
    public class ServiceResponse<TPayload>
    {
        [JsonPropertyName("payload")]
        public TPayload Payload { get; set; }

        [JsonPropertyName("error")]
        public int? Error { get; set; }
    }
}