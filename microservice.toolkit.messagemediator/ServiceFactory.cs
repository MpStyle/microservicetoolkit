namespace microservice.toolkit.messagemediator
{
    /// <summary>
    /// Delegate used by the implementations of service mediator to know how retrieve the instances of the micro-service.
    /// </summary>
    /// <param name="pattern">The name of the micro-service</param>
    /// <returns>The instance of the micro-service</returns>
    public delegate IService ServiceFactory(string pattern);
}
