﻿using microservice.toolkit.messagemediator.attribute;
using microservice.toolkit.messagemediator.entity;
using microservice.toolkit.messagemediator.extension;

using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test;

[ExcludeFromCodeCoverage]
public class LocalMessageMediatorTest
{
    [Test]
    public async Task Run_InvalidPattern_ReturnsError()
    {
        var mediator =
            new LocalMessageMediator(_ => null,
                new NullLogger<LocalMessageMediator>());
        await mediator.Init(CancellationToken.None);

        Assert.That(ServiceError.InvalidPattern, Is.EqualTo((await mediator.Send<int>(null, 2)).Error));
    }
    
    [Test]
    public async Task Run_InvalidMessage_ReturnsError()
    {
        var mediator =
            new LocalMessageMediator(name => typeof(SquarePow).ToPattern().Equals(name) ? new SquarePow() : null,
                new NullLogger<LocalMessageMediator>());
        await mediator.Init(CancellationToken.None);

        Assert.That(ServiceError.NullRequest, Is.EqualTo((await mediator.Send<int>(typeof(SquarePow).ToPattern(), null)).Error));
    }
    
    [Test]
    public async Task Run_ExceptionWhileRunning_ReturnsError()
    {
        var mediator =
            new LocalMessageMediator(name => typeof(ExceptionService).ToPattern().Equals(name) ? new ExceptionService() : null,
                new NullLogger<LocalMessageMediator>());
        await mediator.Init(CancellationToken.None);

        Assert.That(ServiceError.InvalidServiceExecution, Is.EqualTo((await mediator.Send<int>(typeof(ExceptionService).ToPattern(), 1)).Error));
    }
    
    [Test]
    public async Task Run_Object_Int()
    {
        var mediator =
            new LocalMessageMediator(name => typeof(SquarePow).ToPattern().Equals(name) ? new SquarePow() : null,
                new NullLogger<LocalMessageMediator>());
        await mediator.Init(CancellationToken.None);

        Assert.That(4, Is.EqualTo((await mediator.Send<int>(typeof(SquarePow).ToPattern(), 2)).Payload));
    }

    [Test]
    public async Task Run_Int_Int()
    {
        IMessageMediator mediator =
            new LocalMessageMediator(name => typeof(SquarePow).ToPattern().Equals(name) ? new SquarePow() : null,
                new NullLogger<LocalMessageMediator>());
        await mediator.Init(CancellationToken.None);

        Assert.That(4, Is.EqualTo((await mediator.Send<int>(typeof(SquarePow).ToPattern(), 2)).Payload));
    }

    [Test]
    public async Task Run_Object_Int_WithError()
    {
        IMessageMediator mediator =
            new LocalMessageMediator(name => nameof(SquarePowError).Equals(name) ? new SquarePowError() : null,
                new NullLogger<LocalMessageMediator>());
        await mediator.Init(CancellationToken.None);

        Assert.That(ServiceError.ServiceNotFound, Is.EqualTo((await mediator.Send<int>(typeof(SquarePowError).ToPattern(), 2)).Error));
    }

    [Test]
    public async Task Run_ServiceNotFound()
    {
        IMessageMediator mediator =
            new LocalMessageMediator(name => typeof(SquarePow).ToPattern().Equals(name) ? new SquarePow() : null,
                new NullLogger<LocalMessageMediator>());
        await mediator.Init(CancellationToken.None);

        var response = await mediator.Send<int>("ServiceNotFound", 2);

        Assert.That(response.Error, Is.EqualTo(ServiceError.ServiceNotFound));
    }

    [Microservice]
    class SquarePow : Service<int, int>
    {
        public override Task<ServiceResponse<int>> RunAsync(int request, CancellationToken cancellationToken = default)
        {
            return this.SuccessfulResponseAsync(request * request);
        }
    }

    [Microservice]
    class SquarePowError : Service<int, int>
    {
        public override Task<ServiceResponse<int>> RunAsync(int request, CancellationToken cancellationToken = default)
        {
            return this.UnsuccessfulResponseAsync<int>("-1");
        }
    }
    
    [Microservice]
    class ExceptionService : Service<int, int>
    {
        public override Task<ServiceResponse<int>> RunAsync(int request, CancellationToken cancellationToken = default)
        {
            throw new Exception("My exception");
        }
    }
}