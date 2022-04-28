using microservice.toolkit.messagemediator.extension;
using microservice.toolkit.messagemediator.test.data;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace microservice.toolkit.messagemediator.test.extension;

[ExcludeFromCodeCoverage]
public class AssemblyScannerExtensionTest
{
    [Test]
    public void GetServices()
    {
        var serviceTypes = Assembly.GetAssembly(typeof(ValidService01)).GetServices();
        Assert.AreEqual(4, serviceTypes.Length);
        Assert.AreEqual(typeof(ValidService01).FullName, serviceTypes[0].FullName);
        Assert.AreEqual(typeof(ValidService02).FullName, serviceTypes[1].FullName);
    }

    [Test]
    public void GetAssemblyServices()
    {
        var serviceTypes = typeof(ValidService01).GetAssemblyServices();
        Assert.AreEqual(4, serviceTypes.Length);
        Assert.AreEqual(typeof(ValidService01).FullName, serviceTypes[0].FullName);
        Assert.AreEqual(typeof(ValidService02).FullName, serviceTypes[1].FullName);
    }
        
    [Test]
    public void AddServices_FromType()
    {
        var serviceCollection = new ServiceCollection().AddServices(typeof(ValidService01).GetAssemblyServices());

        Assert.IsTrue(serviceCollection.Any(s => s.ImplementationType == typeof(ValidService01)));
        Assert.IsTrue(serviceCollection.Any(s => s.ImplementationType == typeof(ValidService02)));
    }
        
    [Test]
    public void AddServices_FromAssembly()
    {
        var serviceCollection = new ServiceCollection().AddServices(Assembly.GetAssembly(typeof(ValidService01)));

        Assert.IsTrue(serviceCollection.Any(s => s.ImplementationType == typeof(ValidService01)));
        Assert.IsTrue(serviceCollection.Any(s => s.ImplementationType == typeof(ValidService02)));
    }
        
    [Test]
    public void AddAssemblyServices()
    {
        var serviceCollection = new ServiceCollection().AddAssemblyServices(typeof(ValidService01));

        Assert.IsTrue(serviceCollection.Any(s => s.ImplementationType == typeof(ValidService01)));
        Assert.IsTrue(serviceCollection.Any(s => s.ImplementationType == typeof(ValidService02)));
    }

    [Test]
    public void AddServices_FromTypes_SingletonLifeTime()
    {
        var serviceCollection = new ServiceCollection()
            .AddAssemblyServices(typeof(ValidService01))
            .BuildServiceProvider();
            
        Assert.AreEqual(serviceCollection.GetService<ValidService01>(), serviceCollection.GetService<ValidService01>());
        Assert.AreSame(serviceCollection.GetService<ValidService01>(), serviceCollection.GetService<ValidService01>());
    }
        
    [Test]
    public void AddServices_FromTypes_TransientLifeTime()
    {
        var serviceCollection = new ServiceCollection()
            .AddAssemblyServices(typeof(ValidService01), ServiceLifetime.Transient)
            .BuildServiceProvider();
            
        Assert.AreNotEqual(serviceCollection.GetService<ValidService01>(), serviceCollection.GetService<ValidService01>());
        Assert.AreNotSame(serviceCollection.GetService<ValidService01>(), serviceCollection.GetService<ValidService01>());
    }
}