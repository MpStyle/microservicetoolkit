using microservice.toolkit.core;
using microservice.toolkit.core.attribute;
using microservice.toolkit.core.entity;
using microservice.toolkit.core.extension;

using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test
{
    [ExcludeFromCodeCoverage]
    public class LocalMessageMediatorTest
    {
        [Test]
        public async Task Run_Object_Int()
        {
            IMessageMediator mediator =
                new LocalMessageMediator(name => nameof(SquarePow).Equals(name) ? new SquarePow() : null,
                    new NullLogger<LocalMessageMediator>());

            Assert.That(4, Is.EqualTo((await mediator.Send<int>(nameof(SquarePow), 2)).Payload));
        }

        [Test]
        public async Task Run_Int_Int()
        {
            IMessageMediator mediator =
                new LocalMessageMediator(name => nameof(SquarePow).Equals(name) ? new SquarePow() : null,
                    new NullLogger<LocalMessageMediator>());

            Assert.That(4, Is.EqualTo((await mediator.Send<int, int>(nameof(SquarePow), 2)).Payload));
        }

        [Test]
        public async Task Run_Object_Int_WithError()
        {
            IMessageMediator mediator =
                new LocalMessageMediator(name => nameof(SquarePowError).Equals(name) ? new SquarePowError() : null,
                    new NullLogger<LocalMessageMediator>());

            Assert.That(-1, Is.EqualTo((await mediator.Send<int>(nameof(SquarePowError), 2)).Error));
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