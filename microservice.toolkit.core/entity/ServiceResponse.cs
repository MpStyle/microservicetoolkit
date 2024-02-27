namespace microservice.toolkit.core.entity
{
    public class ServiceResponse<TPayload>
    {
        public TPayload Payload { get; set; }

        public int? Error { get; set; }
    }
}