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

        Assert.That(5, Is.EqualTo(serviceTypes.Count));

        Assert.That(serviceTypes.ContainsPattern(typeof(ValidService01).ToPattern()), Is.True);
        Assert.That(serviceTypes[typeof(ValidService01).ToPattern()].First() == typeof(ValidService01), Is.True);

        Assert.That(serviceTypes.ContainsPattern(typeof(ValidService02).ToPattern()), Is.True);
        Assert.That(serviceTypes[typeof(ValidService02).ToPattern()].First() == typeof(ValidService02), Is.True);
    }

    [Test]
    public void GetServices_ByType()
    {
        var serviceTypes = typeof(ValidService01).GetServices();

        Assert.That(5, Is.EqualTo(serviceTypes.Count));

        Assert.That(serviceTypes.ContainsPattern(typeof(ValidService01).ToPattern()), Is.True);
        Assert.That(serviceTypes[typeof(ValidService01).ToPattern()].First() == typeof(ValidService01), Is.True);

        Assert.That(serviceTypes.ContainsPattern(typeof(ValidService02).ToPattern()), Is.True);
        Assert.That(serviceTypes[typeof(ValidService02).ToPattern()].First() == typeof(ValidService02), Is.True);
    }

    [Test]
    public void AddServices_SingletonLifeTime()
    {
        var serviceCollection = new ServiceCollection()
            .AddServiceContext(typeof(ValidService01))
            .BuildServiceProvider();

        Assert.That(serviceCollection.GetService<ValidService01>(), Is.EqualTo(serviceCollection.GetService<ValidService01>()));
        Assert.That(serviceCollection.GetService<ValidService01>(), Is.SameAs(serviceCollection.GetService<ValidService01>()));
    }

    [Test]
    public void AddServices_TransientLifeTime()
    {
        var serviceCollection = new ServiceCollection()
            .AddServiceContext(typeof(ValidService01), ServiceLifetime.Transient)
            .BuildServiceProvider();

        Assert.That(serviceCollection.GetService<ValidService01>(), Is.Not.EqualTo(serviceCollection.GetService<ValidService01>()));
        Assert.That(serviceCollection.GetService<ValidService01>(), Is.Not.SameAs(serviceCollection.GetService<ValidService01>()));
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
        Assert.That(instance01 is ValidService03, Is.True);

        var instance02 = serviceFactory(nameof(ValidService04));
        Assert.That(instance02 is ValidService04, Is.True);
    }
}
