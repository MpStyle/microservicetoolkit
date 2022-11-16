using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test;

[ExcludeFromCodeCoverage]
public class LocalSignalEmitterTest
{
    private static bool isSignalHandlerRunned = false;

    [Test]
    public async Task Run_Int()
    {
        ISignalEmitter signalEmitter =
            new LocalSignalEmitter(name => nameof(SquarePow).Equals(name) ? new SquarePow() : null,
                new NullLogger<LocalSignalEmitter>());

        await signalEmitter.Emit(nameof(SquarePow), 2);

        Assert.IsFalse(isSignalHandlerRunned);

        await Task.Delay(5000);

        Assert.IsTrue(isSignalHandlerRunned);
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

