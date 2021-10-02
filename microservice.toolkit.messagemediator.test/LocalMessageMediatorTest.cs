using microservice.toolkit.core;
using microservice.toolkit.core.entity;
using microservice.toolkit.messagemediator;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.test.book.messagemediator
{
    [ExcludeFromCodeCoverage]
    public class LocalMessageMediatorTest
    {
        [Test]
        public async Task Run_Object_Object()
        {
            IMessageMediator mediator = new LocalMessageMediator(name =>
            {
                if (nameof(SquarePow).Equals(name))
                {
                    return new SquarePow();
                }

                return null;
            });

            Assert.AreEqual(4, (await mediator.Send(nameof(SquarePow), 2)).Payload);
        }

        [Test]
        public async Task Run_Object_Int()
        {
            IMessageMediator mediator = new LocalMessageMediator(name =>
            {
                if (nameof(SquarePow).Equals(name))
                {
                    return new SquarePow();
                }

                return null;
            });

            Assert.AreEqual(4, (await mediator.Send<int>(nameof(SquarePow), 2)).Payload);
        }

        [Test]
        public async Task Run_Int_Int()
        {
            IMessageMediator mediator = new LocalMessageMediator(name =>
            {
                if (nameof(SquarePow).Equals(name))
                {
                    return new SquarePow();
                }

                return null;
            });

            Assert.AreEqual(4, (await mediator.Send<int, int>(nameof(SquarePow), 2)).Payload);
        }

        [Test]
        public async Task Run_Object_Object_WithError()
        {
            IMessageMediator mediator = new LocalMessageMediator(name =>
            {
                if (nameof(SquarePowError).Equals(name))
                {
                    return new SquarePowError();
                }

                return null;
            });

            Assert.AreEqual(-1, (await mediator.Send(nameof(SquarePowError), 2)).Error);
        }

        class SquarePow : Service<int, int>
        {
            public async override Task<ServiceResponse<int>> Run(int request)
            {
                return this.SuccessfulResponse(request * request);
            }
        }

        class SquarePowError : Service<int, int>
        {
            public async override Task<ServiceResponse<int>> Run(int request)
            {
                return this.UnsuccessfulResponse(-1);
            }
        }
    }
}
