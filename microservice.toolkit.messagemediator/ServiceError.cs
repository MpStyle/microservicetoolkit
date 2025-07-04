namespace microservice.toolkit.messagemediator;

public static class ServiceError
{
    public const string Unknown = "mt_0";
    public const string ServiceNotFound = "mt_1";
    public const string InvalidPattern = "mt_2";
    public const string InvalidServiceExecution = "mt_4";
    public const string ExecutionTimeout = "mt_5";
    public const string EmptyResponse = "mt_6";
    public const string EmptyRequest = "mt_7";
    public const string Timeout = "mt_8";
    public const string InvalidRequestType = "mt_9";
    public const string NullRequest = "mt_10";
    public const string NullResponse = "mt_11";
    public const string ResponseDeserializationError = "mt_12";
    public const string RequestDeserializationError = "mt_13";
}
