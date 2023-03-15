using microservice.toolkit.core.extension;
using microservice.toolkit.messagemediator.extension;
using microservice.toolkit.messagemediator.test.data;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace microservice.toolkit.messagemediator.test.extension;

[ExcludeFromCodeCoverage]
public class MessageMediatorExtensionsTest
{
    [Test]
    public void GetServices_ByAssembly()
    {
        var serviceTypes = Assembly.GetAssembly(typeof(ValidService01)).GetServices();

        Assert.AreEqual(5, serviceTypes.Count);

        Assert.IsTrue(serviceTypes.ContainsPattern(typeof(ValidService01).ToPattern()));
        Assert.IsTrue(serviceTypes[typeof(ValidService01).ToPattern()].First() == typeof(ValidService01));

        Assert.IsTrue(serviceTypes.ContainsPattern(typeof(ValidService02).ToPattern()));
        Assert.IsTrue(serviceTypes[typeof(ValidService02).ToPattern()].First() == typeof(ValidService02));
    }

    [Test]
    public void GetServices_ByType()
    {
        var serviceTypes = typeof(ValidService01).GetServices();

        Assert.AreEqual(5, serviceTypes.Count);

        Assert.IsTrue(serviceTypes.ContainsPattern(typeof(ValidService01).ToPattern()));
        Assert.IsTrue(serviceTypes[typeof(ValidService01).ToPattern()].First() == typeof(ValidService01));

        Assert.IsTrue(serviceTypes.ContainsPattern(typeof(ValidService02).ToPattern()));
        Assert.IsTrue(serviceTypes[typeof(ValidService02).ToPattern()].First() == typeof(ValidService02));
    }

    [Test]
    public void AddServices_SingletonLifeTime()
    {
        var serviceCollection = new ServiceCollection()
            .AddServiceContext(typeof(ValidService01))
            .BuildServiceProvider();

        Assert.AreEqual(serviceCollection.GetService<ValidService01>(), serviceCollection.GetService<ValidService01>());
        Assert.AreSame(serviceCollection.GetService<ValidService01>(), serviceCollection.GetService<ValidService01>());
    }

    [Test]
    public void AddServices_TransientLifeTime()
    {
        var serviceCollection = new ServiceCollection()
            .AddServiceContext(typeof(ValidService01), ServiceLifetime.Transient)
            .BuildServiceProvider();

        Assert.AreNotEqual(serviceCollection.GetService<ValidService01>(), serviceCollection.GetService<ValidService01>());
        Assert.AreNotSame(serviceCollection.GetService<ValidService01>(), serviceCollection.GetService<ValidService01>());
    }

    [Test]
    public void ByNameServiceFactory()
    {
        var types = new[]
        {
            typeof(ValidService04)
        };

        var serviceProvider = new ServiceCollection()
            .AddServiceContext(types)
            .BuildServiceProvider();

        var serviceFactory = serviceProvider.GetService<ServiceFactory>();

        var instance01 = serviceFactory(nameof(ValidService03));
        Assert.IsTrue(instance01 is ValidService03);

        var instance02 = serviceFactory(nameof(ValidService04));
        Assert.IsTrue(instance02 is ValidService04);
    }
}
