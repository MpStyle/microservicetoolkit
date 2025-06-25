using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test;

[ExcludeFromCodeCoverage]
public class RabbitMQEventEmitterTest
{
    private readonly RabbitMQSignalEmitterConfiguration configuration = new("test_queue", "localhost");

    private static bool isSignalHandlerRunned;
    private RabbitMQSignalEmitter signalEmitter;

    [Test]
    public async Task Run_Int()
    {
        this.signalEmitter = new RabbitMQSignalEmitter(configuration,
            name => nameof(SquarePow).Equals(name) ? [new SquarePow()] : null,
            new NullLogger<RabbitMQSignalEmitter>());
        await this.signalEmitter.Init(CancellationToken.None);

        await signalEmitter.EmitAsync(nameof(SquarePow), 2);

        Assert.That(isSignalHandlerRunned, Is.False);

        await Task.Delay(5000);

        Assert.That(isSignalHandlerRunned, Is.True);
    }

    [SetUp]
    public void SetUp()
    {
        isSignalHandlerRunned = false;
    }

    [TearDown]
    public async Task TearDown()
    {
        try
        {
            if (this.signalEmitter != null)
            {
                await this.signalEmitter.Shutdown(CancellationToken.None);
                this.signalEmitter = null;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    class SquarePow : SignalHandler<int>
    {
        public override async Task Run(int request, CancellationToken cancellationToken)
        {
            await Task.Delay(3000, cancellationToken);
            isSignalHandlerRunned = true;
        }
    }
}