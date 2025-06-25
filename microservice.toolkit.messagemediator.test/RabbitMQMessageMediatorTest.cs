using microservice.toolkit.core.attribute;
using microservice.toolkit.messagemediator.entity;

using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using static microservice.toolkit.messagemediator.utils.ServiceUtils;

namespace microservice.toolkit.messagemediator.test
{
    [ExcludeFromCodeCoverage]
    public class RabbitMQMessageMediatorTest
    {
        private readonly RabbitMQMessageMediatorConfiguration configuration = new($"test_queue_{Guid.NewGuid()}",
            $"test_reply_queue_{Guid.NewGuid()}", "localhost");

        private IMessageMediator mediator;
        private IMessageMediator mediator01;
        private IMessageMediator mediator02;

        [Test]
        public async Task Run_Object_Int()
        {
            this.mediator = new RabbitMQMessageMediator(configuration,
                name => nameof(SquarePow).Equals(name) ? new SquarePow() : null,
                new NullLogger<RabbitMQMessageMediator>());
            await this.mediator.Init(CancellationToken.None);

            Assert.That(4, Is.EqualTo((await mediator.Send<int>(nameof(SquarePow), 2)).Payload));
        }

        [Test]
        public async Task Run_Int_Int()
        {
            this.mediator = new RabbitMQMessageMediator(configuration,
                name => nameof(SquarePow).Equals(name) ? new SquarePow() : null,
                new NullLogger<RabbitMQMessageMediator>());
            await this.mediator.Init(CancellationToken.None);

            Assert.That(4, Is.EqualTo((await mediator.Send<int>(nameof(SquarePow), 2)).Payload));
        }

        [Test]
        public async Task Run_Object_Int_WithError()
        {
            this.mediator = new RabbitMQMessageMediator(configuration,
                name => nameof(SquarePowError).Equals(name) ? new SquarePowError() : null,
                new NullLogger<RabbitMQMessageMediator>());
            await this.mediator.Init(CancellationToken.None);

            Assert.That(-1, Is.EqualTo((await mediator.Send<int>(nameof(SquarePowError), 2)).Error));
        }

        [Test]
        public async Task MultipleRun()
        {
            this.mediator01 = new RabbitMQMessageMediator(configuration,
                name => nameof(SquarePow).Equals(name) ? new SquarePow() : null,
                new NullLogger<RabbitMQMessageMediator>());
            await this.mediator01.Init(CancellationToken.None);

            this.mediator02 = new RabbitMQMessageMediator(configuration,
                name => nameof(SquarePow).Equals(name) ? new SquarePow() : null,
                new NullLogger<RabbitMQMessageMediator>());
            await this.mediator02.Init(CancellationToken.None);

            Assert.That(4, Is.EqualTo((await mediator01.Send<int>(nameof(SquarePow), 2)).Payload));
            Assert.That(4, Is.EqualTo((await mediator02.Send<int>(nameof(SquarePow), 2)).Payload));
            Assert.That(9, Is.EqualTo((await mediator01.Send<int>(nameof(SquarePow), 3)).Payload));
            Assert.That(9, Is.EqualTo((await mediator02.Send<int>(nameof(SquarePow), 3)).Payload));
            Assert.That(16, Is.EqualTo((await mediator01.Send<int>(nameof(SquarePow), 4)).Payload));
            Assert.That(16, Is.EqualTo((await mediator02.Send<int>(nameof(SquarePow), 4)).Payload));
            Assert.That(25, Is.EqualTo((await mediator01.Send<int>(nameof(SquarePow), 5)).Payload));
            Assert.That(25, Is.EqualTo((await mediator02.Send<int>(nameof(SquarePow), 5)).Payload));
        }

        [TearDown]
        public async Task TearDown()
        {
            try
            {
                if (this.mediator != null)
                {
                    await this.mediator.Shutdown(CancellationToken.None);
                    this.mediator = null;
                }

                if (this.mediator01 != null)
                {
                    await this.mediator01.Shutdown(CancellationToken.None);
                    this.mediator01 = null;
                }

                if (this.mediator02 != null)
                {
                    await this.mediator02.Shutdown(CancellationToken.None);
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
            public override Task<ServiceResponse<int>> RunAsync(int request, CancellationToken cancellationToken = default)
            {
                return SuccessfulResponseAsync(request * request);
            }
        }

        [Microservice]
        class SquarePowError : Service<int, int>
        {
            public override Task<ServiceResponse<int>> RunAsync(int request, CancellationToken cancellationToken = default)
            {
                return UnsuccessfulResponseAsync<int>(-1);
            }
        }
    }
}