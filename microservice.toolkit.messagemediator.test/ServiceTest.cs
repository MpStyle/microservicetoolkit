using microservice.toolkit.messagemediator.entity;
using microservice.toolkit.messagemediator.extension;

using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test;

[ExcludeFromCodeCoverage]
public class ServiceTest
{
    [Test]
    public async Task Run_With_exception()
    {
        var mediator = new LocalMessageMediator(_ => new MyServiceWithException(),
            new NullLogger<LocalMessageMediator>());

        var response = await mediator.Send<int>("any_service", 2);

        Assert.That(response.Error, Is.EqualTo(ServiceError.InvalidServiceExecution));
    }

    [Test]
    public async Task Run_SuccessfulResponseTask()
    {
        var service = new MyServiceSuccessfulResponseTask();
        var response = await service.RunAsync(10);
        Assert.That(response.Payload, Is.EqualTo(10));
    }

    [Test]
    public async Task Run_UnsuccessfulResponseTask()
    {
        var service = new MyServiceUnsuccessfulResponseTask();
        var response = await service.RunAsync(10);
        Assert.That(response.Error, Is.EqualTo("10"));
    }

    [Test]
    public async Task Run_ResponseTask()
    {
        var service = new MyServiceResponseTask();
        var response = await service.RunAsync(10);
        Assert.That(response.Payload, Is.EqualTo(0));
        Assert.That(response.Error, Is.EqualTo("20"));
    }
}

[ExcludeFromCodeCoverage]
class MyServiceWithException : Service<int, int>
{
    public override Task<ServiceResponse<int>> RunAsync(int request, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}

[ExcludeFromCodeCoverage]
class MyServiceSuccessfulResponseTask : Service<int, int>
{
    public override Task<ServiceResponse<int>> RunAsync(int request, CancellationToken cancellationToken = default)
    {
        return this.SuccessfulResponseAsync(request);
    }
}

[ExcludeFromCodeCoverage]
class MyServiceUnsuccessfulResponseTask : Service<int, int>
{
    public override Task<ServiceResponse<int>> RunAsync(int request, CancellationToken cancellationToken = default)
    {
        return this.UnsuccessfulResponseAsync<int>(request.ToString());
    }
}

[ExcludeFromCodeCoverage]
class MyServiceResponseTask : Service<int, int>
{
    public override Task<ServiceResponse<int>> RunAsync(int request, CancellationToken cancellationToken = default)
    {
        return this.ResponseAsync(request / 2, (request * 2).ToString());
    }
}