using microservice.toolkit.messagemediator.utils;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test.utils;

[ExcludeFromCodeCoverage]
public class ServiceUtilsTest
{
    [Test]
    public void SuccessfulResponse_ShouldReturnResponseWithPayload_AndNullError()
    {
        var payload = "test";
        var response = ServiceUtils.SuccessfulResponse(payload);

        Assert.That(response.Payload, Is.EqualTo(payload));
        Assert.That(response.Error, Is.Null);
    }

    [Test]
    public async Task SuccessfulResponseAsync_ShouldReturnResponseWithPayload_AndNullError()
    {
        var payload = 42;
        var response = await ServiceUtils.SuccessfulResponseAsync(payload);

        Assert.That(response.Payload, Is.EqualTo(payload));
        Assert.That(response.Error, Is.Null);
    }

    [Test]
    public void UnsuccessfulResponse_ShouldReturnResponseWithError_AndNullPayload()
    {
        var error = "123";
        var response = ServiceUtils.UnsuccessfulResponse<string>(error);

        Assert.That(response.Error, Is.EqualTo(error));
        Assert.That(response.Payload, Is.Null);
    }

    [Test]
    public async Task UnsuccessfulResponseAsync_ShouldReturnResponseWithError_AndNullPayload()
    {
        var error = "456";
        var response = await ServiceUtils.UnsuccessfulResponseAsync<string>(error);

        Assert.That(response.Error, Is.EqualTo(error));
        Assert.That(response.Payload, Is.Null);
    }

    [Test]
    public void Response_ShouldReturnResponseWithPayload_WhenErrorIsNull()
    {
        const string payload = "payload";
        string? error = null;
        var response = ServiceUtils.Response(payload, error);

        Assert.That(response.Payload, Is.EqualTo(payload));
        Assert.That(response.Error, Is.Null);
    }

    [Test]
    public void Response_ShouldReturnResponseWithError_WhenErrorIsNotNull()
    {
        const string payload = "payload";
        const string? error = "99";
        var response = ServiceUtils.Response(payload, error);

        Assert.That(response.Payload, Is.Null);
        Assert.That(response.Error, Is.EqualTo(error));
    }

    [Test]
    public async Task ResponseAsync_ShouldReturnResponseWithPayload_WhenErrorIsNull()
    {
        const int payload = 100;
        string? error = null;
        var response = await ServiceUtils.ResponseAsync(payload, error);

        Assert.That(response.Payload, Is.EqualTo(payload));
        Assert.That(response.Error, Is.Null);
    }

    [Test]
    public async Task ResponseAsync_ShouldReturnResponseWithError_WhenErrorIsNotNull()
    {
        int? payload = 200;
        const string? error = "77";
        var response = await ServiceUtils.ResponseAsync(payload, error);

        Assert.That(response.Payload, Is.Null);
        Assert.That(response.Error, Is.EqualTo(error));
    }
}
