using microservice.toolkit.core.entity;
using microservice.toolkit.core.extension;

using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test;

[ExcludeFromCodeCoverage]
public class ServiceTest
{
    [Test]
    public async Task Run_With_exception()
    {
        var mediator = new LocalMessageMediator(pattern => new MyServiceWithException(),
            new NullLogger<LocalMessageMediator>());

        var response = await mediator.Send<int>("any_service", 2);

        Assert.That(response.Error, Is.EqualTo(ServiceError.InvalidServiceExecution));
    }

    [Test]
    public async Task Run_SuccessfulResponseTask()
    {
        var service = new MyServiceSuccessfulResponseTask();
        var response = await service.Run(10);
        Assert.That(response.Payload, Is.EqualTo(10));
    }

    [Test]
    public async Task Run_UnsuccessfulResponseTask()
    {
        var service = new MyServiceUnsuccessfulResponseTask();
        var response = await service.Run(10);
        Assert.That(response.Error, Is.EqualTo(10));
    }

    [Test]
    public async Task Run_ResponseTask()
    {
        var service = new MyServiceResponseTask();
        var response = await service.Run(10);
        Assert.That(response.Payload, Is.EqualTo(0));
        Assert.That(response.Error, Is.EqualTo(20));
    }
}

class MyServiceWithException : Service<int, int>
{
    public override Task<ServiceResponse<int>> Run(int request)
    {
        throw new System.NotImplementedException();
    }
}

class MyServiceSuccessfulResponseTask : Service<int, int>
{
    public override Task<ServiceResponse<int>> Run(int request)
    {
        return this.SuccessfulResponseTask(request);
    }
}

class MyServiceUnsuccessfulResponseTask : Service<int, int>
{
    public override Task<ServiceResponse<int>> Run(int request)
    {
        return this.UnsuccessfulResponseTask(request);
    }
}

class MyServiceResponseTask : Service<int, int>
{
    public override Task<ServiceResponse<int>> Run(int request)
    {
        return this.ResponseTask(request / 2, request * 2);
    }
}