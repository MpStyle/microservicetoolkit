﻿using microservice.toolkit.messagemediator.utils;

using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Threading;
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
            new LocalSignalEmitter(name => SignalHandlerUtils.PatternOf<SquarePow>().Equals(name) ? [new SquarePow()] : null,
                new NullLogger<LocalSignalEmitter>());
        await signalEmitter.Init();

        await signalEmitter.Emit(SignalHandlerUtils.PatternOf<SquarePow>(), 2);

        Assert.That(isSignalHandlerRunned, Is.False);

        await Task.Delay(5000);

        Assert.That(isSignalHandlerRunned, Is.True);
    }

    [Test]
    public async Task Run_Not_found()
    {
        var signalEmitter =
            new LocalSignalEmitter(name => "ServiceNotFound".Equals(name) ? [new SquarePow()] : null,
                new NullLogger<LocalSignalEmitter>());
        await signalEmitter.Init();

        await signalEmitter.Emit(SignalHandlerUtils.PatternOf<SquarePow>(), 2);

        Assert.That(isSignalHandlerRunned, Is.False);

        await Task.Delay(5000);

        Assert.That(isSignalHandlerRunned, Is.False);
    }

    [SetUp]
    public void SetUp()
    {
        isSignalHandlerRunned = false;
    }

    class SquarePow : SignalHandler<int>
    {
        public override async Task Run(int request, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);
            isSignalHandlerRunned = true;
        }
    }
}