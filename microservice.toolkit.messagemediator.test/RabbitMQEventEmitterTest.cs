using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test
{
    [ExcludeFromCodeCoverage]
    public class RabbitMQEventEmitterTest
    {
        private readonly RabbitMQSignalEmitterConfiguration configuration = new()
        {
            ConnectionString = "localhost", QueueName = "test_queue"
        };

        private static bool isSignalHandlerRunned;
        private RabbitMQSignalEmitter signalEmitter;

        [Test]
        public async Task Run_Int()
        {
            this.signalEmitter = new RabbitMQSignalEmitter(configuration,
                name => nameof(SquarePow).Equals(name) ? new ISignalHandler[] { new SquarePow() } : null,
                new NullLogger<RabbitMQSignalEmitter>());

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

        [TearDown]
        public async Task TearDown()
        {
            try
            {
                if (this.signalEmitter != null)
                {
                    await this.signalEmitter.Shutdown();
                    this.signalEmitter = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        class SquarePow : SignalHandler<int>
        {
            public override async Task Run(int request)
            {
                await Task.Delay(3000);
                isSignalHandlerRunned = true;
            }
        }
    }
}