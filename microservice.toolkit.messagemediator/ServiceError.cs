namespace microservice.toolkit.messagemediator
{
    public static class ServiceError
    {
        public const int Unknown = 8000;
        public const int ServiceNotFound = 8001;
        public const int InvalidService = 8002;
        public const int InvalidServiceExecution = 8004;
        public const int ExecutionTimeout = 8005;
        public const int EmptyResponse = 8006;
        public const int EmptyRequest = 8007;
        public const int TimeOut = 8008;
        public const int InvalidRequestType = 8009;
    }
}
