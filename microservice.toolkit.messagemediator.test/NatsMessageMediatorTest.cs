﻿using microservice.toolkit.core.entity;
using microservice.toolkit.messagemediator.attribute;

using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test
{
    [ExcludeFromCodeCoverage]
    public class NatsMessageMediatorTest
    {
        private readonly NatsMessageMediatorConfiguration configuration = new NatsMessageMediatorConfiguration
        {
            ConnectionString = "localhost:4222",
            Topic = $"test_topic_{Guid.NewGuid()}",
        };

        private IMessageMediator mediator;
        private IMessageMediator mediator01;
        private IMessageMediator mediator02;

        [Test]
        public async Task Run_Object_Int()
        {
            this.mediator = new NatsMessageMediator(configuration,
                name => nameof(SquarePow).Equals(name) ? new SquarePow() : null,
                new NullLogger<NatsMessageMediator>());

            Assert.AreEqual(4, (await mediator.Send<int>(nameof(SquarePow), 2)).Payload);
        }

        [Test]
        public async Task Run_Int_Int()
        {
            this.mediator = new NatsMessageMediator(configuration,
                name => nameof(SquarePow).Equals(name) ? new SquarePow() : null,
                new NullLogger<NatsMessageMediator>());

            Assert.AreEqual(4, (await mediator.Send<int, int>(nameof(SquarePow), 2)).Payload);
        }

        [Test]
        public async Task Run_Object_Int_WithError()
        {
            this.mediator = new NatsMessageMediator(configuration,
                name => nameof(SquarePowError).Equals(name) ? new SquarePowError() : null,
                new NullLogger<NatsMessageMediator>());

            Assert.AreEqual(-1, (await mediator.Send<int>(nameof(SquarePowError), 2)).Error);
        }

        [Test]
        public async Task MultipleRun()
        {
            this.mediator01 = new NatsMessageMediator(configuration,
                name => nameof(SquarePow).Equals(name) ? new SquarePow() : null,
                new NullLogger<NatsMessageMediator>());
            this.mediator02 = new NatsMessageMediator(configuration,
                name => nameof(SquarePow).Equals(name) ? new SquarePow() : null,
                new NullLogger<NatsMessageMediator>());

            Assert.AreEqual(4, (await mediator01.Send<int, int>(nameof(SquarePow), 2)).Payload);
            Assert.AreEqual(4, (await mediator02.Send<int, int>(nameof(SquarePow), 2)).Payload);
            Assert.AreEqual(9, (await mediator01.Send<int, int>(nameof(SquarePow), 3)).Payload);
            Assert.AreEqual(9, (await mediator02.Send<int, int>(nameof(SquarePow), 3)).Payload);
            Assert.AreEqual(16, (await mediator01.Send<int, int>(nameof(SquarePow), 4)).Payload);
            Assert.AreEqual(16, (await mediator02.Send<int, int>(nameof(SquarePow), 4)).Payload);
            Assert.AreEqual(25, (await mediator01.Send<int, int>(nameof(SquarePow), 5)).Payload);
            Assert.AreEqual(25, (await mediator02.Send<int, int>(nameof(SquarePow), 5)).Payload);
        }

        [TearDown]
        public async Task TearDown()
        {
            try
            {
                if (this.mediator != null)
                {
                    await this.mediator.Shutdown();
                    this.mediator = null;
                }

                if (this.mediator01 != null)
                {
                    await this.mediator01.Shutdown();
                    this.mediator01 = null;
                }

                if (this.mediator02 != null)
                {
                    await this.mediator02.Shutdown();
                    this.mediator02 = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [Microservice]
        class SquarePow : Service<int, int>
        {
            public override Task<ServiceResponse<int>> Run(int request)
            {
                return Task.FromResult(this.SuccessfulResponse(request * request));
            }
        }

        [Microservice]
        class SquarePowError : Service<int, int>
        {
            public override Task<ServiceResponse<int>> Run(int request)
            {
                return Task.FromResult(this.UnsuccessfulResponse(-1));
            }
        }
    }
}