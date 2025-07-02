namespace microservice.toolkit.messagemediator.entity;

public record ServiceResponse<TPayload>
{
    public TPayload? Payload { get; set; }

    public string? Error { get; set; }
}