using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test;

[ExcludeFromCodeCoverage]
public class LocalSignalEmitterTest
{
    private static bool isSignalHandlerRunned;

    [Test]
    public async Task Run_Int()
    {
        var signalEmitter =
            new LocalSignalEmitter(name => nameof(SquarePow).Equals(name) ? [new SquarePow()] : null,
                new NullLogger<LocalSignalEmitter>());

        await signalEmitter.Emit(nameof(SquarePow), 2);

        Assert.That(isSignalHandlerRunned, Is.False);

        await Task.Delay(5000);

        Assert.That(isSignalHandlerRunned, Is.True);
    }

    [SetUp]
    public void SetUp()
    {
        isSignalHandlerRunned = false;
    }

    class SquarePow : SignalHandler<int>
    {
        public override async Task Run(int request)
        {
            await Task.Delay(1000);
            isSignalHandlerRunned = true;
        }
    }
}