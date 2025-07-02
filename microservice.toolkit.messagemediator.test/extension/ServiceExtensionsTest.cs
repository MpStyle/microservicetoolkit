using microservice.toolkit.messagemediator.entity;
using microservice.toolkit.messagemediator.extension;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test.extension;

[ExcludeFromCodeCoverage]
public class ServiceExtensionsTest
{
    [Test]
    public void SuccessfulResponse_OnIService()
    {
        IService service = new MyService();
        var response = service.SuccessfulResponse(1);
        Assert.That(1, Is.EqualTo(response.Payload));
    }
    
    [Test]
    public void SuccessfulResponseAsync_OnIService()
    {
        IService service = new MyService();
        var response = service.SuccessfulResponseAsync(1).Result;
        Assert.That(1, Is.EqualTo(response.Payload));
    }
    
    [Test]
    public void UnsuccessfulResponse_OnIService()
    {
        IService service = new MyService();
        var response = service.UnsuccessfulResponse<int>("Error");
        Assert.That("Error", Is.EqualTo(response.Error));
    }
    
    [Test]
    public void UnsuccessfulResponseAsync_OnIService()
    {
        IService service = new MyService();
        var response = service.UnsuccessfulResponseAsync<int>("Error").Result;
        Assert.That("Error", Is.EqualTo(response.Error));
    }
    
    [Test]
    public void Response_OnIService()
    {
        IService service = new MyService();
        var response = service.Response(1, "Error");
        Assert.That(0, Is.EqualTo(response.Payload));
        Assert.That("Error", Is.EqualTo(response.Error));
    }

    [Test]
    public void ResponseAsync_OnIService()
    {
        IService service = new MyService();
        var response = service.ResponseAsync(1, "Error").Result;
        Assert.That(0, Is.EqualTo(response.Payload));
        Assert.That("Error", Is.EqualTo(response.Error));
    }
    
    [Test]
    public void SuccessfulResponse_OnService()
    {
        Service<int, int> service = new MyService();
        var response = service.SuccessfulResponse(1);
        Assert.That(1, Is.EqualTo(response.Payload));
    }
    
    [Test]
    public void SuccessfulResponseAsync_OnService()
    {
        Service<int, int> service = new MyService();
        var response = service.SuccessfulResponseAsync(1).Result;
        Assert.That(1, Is.EqualTo(response.Payload));
    }
    
    [Test]
    public void UnsuccessfulResponse_OnService()
    {
        Service<int, int> service = new MyService();
        var response = service.UnsuccessfulResponse<int>("Error");
        Assert.That("Error", Is.EqualTo(response.Error));
    }
    
    [Test]
    public void UnsuccessfulResponseAsync_OnService()
    {
        Service<int, int> service = new MyService();
        var response = service.UnsuccessfulResponseAsync<int>("Error").Result;
        Assert.That("Error", Is.EqualTo(response.Error));
    }
    
    [Test]
    public void Response_OnService()
    {
        Service<int, int> service = new MyService();
        var response = service.Response(1, "Error");
        Assert.That(0, Is.EqualTo(response.Payload));
        Assert.That("Error", Is.EqualTo(response.Error));
    }
    
    [Test]
    public void ResponseAsync_OnService()
    {
        Service<int, int> service = new MyService();
        var response = service.ResponseAsync(1, "Error").Result;
        Assert.That(0, Is.EqualTo(response.Payload));
        Assert.That("Error", Is.EqualTo(response.Error));
    }
    
    private class MyService : Service<int, int>
    {
        public override Task<ServiceResponse<int>> RunAsync(int request, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}