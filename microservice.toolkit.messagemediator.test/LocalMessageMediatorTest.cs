using microservice.toolkit.core;
using microservice.toolkit.core.attribute;
using microservice.toolkit.core.entity;
using microservice.toolkit.core.extension;

using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using static microservice.toolkit.messagemediator.utils.ServiceUtils;

namespace microservice.toolkit.messagemediator.test
{
    [ExcludeFromCodeCoverage]
    public class LocalMessageMediatorTest
    {
        [Test]
        public async Task Run_Object_Int()
        {
            var mediator =
                new LocalMessageMediator(name => typeof(SquarePow).ToPattern().Equals(name) ? new SquarePow() : null,
                    new NullLogger<LocalMessageMediator>());
            await mediator.InitAsync(CancellationToken.None);

            Assert.That(4, Is.EqualTo((await mediator.SendAsync<int>(typeof(SquarePow).ToPattern(), 2)).Payload));
        }

        [Test]
        public async Task Run_Int_Int()
        {
            IMessageMediator mediator =
                new LocalMessageMediator(name =>typeof(SquarePow).ToPattern().Equals(name) ? new SquarePow() : null,
                    new NullLogger<LocalMessageMediator>());
            await mediator.InitAsync(CancellationToken.None);

            Assert.That(4, Is.EqualTo((await mediator.Send<int, int>(typeof(SquarePow).ToPattern(), 2)).Payload));
        }

        [Test]
        public async Task Run_Object_Int_WithError()
        {
            IMessageMediator mediator =
                new LocalMessageMediator(name => nameof(SquarePowError).Equals(name) ? new SquarePowError() : null,
                    new NullLogger<LocalMessageMediator>());
            await mediator.InitAsync(CancellationToken.None);

            Assert.That(ServiceError.ServiceNotFound, Is.EqualTo((await mediator.SendAsync<int>(typeof(SquarePowError).ToPattern(), 2)).Error));
        }

        [Test]
        public async Task Run_ServiceNotFound()
        {
            IMessageMediator mediator =
                new LocalMessageMediator(name => typeof(SquarePow).ToPattern().Equals(name) ? new SquarePow() : null,
                    new NullLogger<LocalMessageMediator>());
            await mediator.InitAsync(CancellationToken.None);

            var response=await mediator.Send<int, int>("ServiceNotFound", 2);
            
            Assert.That(response.Error, Is.EqualTo(ServiceError.ServiceNotFound));
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