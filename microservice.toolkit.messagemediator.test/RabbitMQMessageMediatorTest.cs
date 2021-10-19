using microservice.toolkit.core;
using microservice.toolkit.core.entity;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test
{
    [ExcludeFromCodeCoverage]
    public class RabbitMQMessageMediatorTest
    {
        private readonly RpcMessageMediatorConfiguration configuration = new RpcMessageMediatorConfiguration
        {
            ConnectionString = "localhost",
            QueueName = "test_queue",
            ReplyQueueName = "test_reply_queue",
            ConsumersPerQueue = 3
        };

        private IMessageMediator mediator;

        [Test]
        public async Task Run_Object_Int()
        {
            this.mediator = new RabbitMQMessageMediator(configuration, name =>
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
            this.mediator = new RabbitMQMessageMediator(configuration, name =>
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
        public async Task Run_Object_Int_WithError()
        {
            this.mediator = new RabbitMQMessageMediator(configuration, name =>
            {
                if (nameof(SquarePowError).Equals(name))
                {
                    return new SquarePowError();
                }

                return null;
            });

            Assert.AreEqual(-1, (await mediator.Send<int>(nameof(SquarePowError), 2)).Error);
        }

        [TearDown]
        public async Task TearDown()
        {
            try
            {
                await this.mediator.Shutdown();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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
