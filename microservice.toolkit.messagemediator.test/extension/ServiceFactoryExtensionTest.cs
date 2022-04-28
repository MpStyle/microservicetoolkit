using microservice.toolkit.messagemediator.extension;
using microservice.toolkit.messagemediator.test.data;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.messagemediator.test.extension;

[ExcludeFromCodeCoverage]
public class ServiceFactoryExtensionTest
{
    [Test]
    public void ByNameServiceFactory()
    {
        var types = new[]
        {
            typeof(ValidService03),
            typeof(ValidService04)
        };
        var serviceProvider = new ServiceCollection()
            .AddServices(types)
            .BuildServiceProvider();
        var serviceFactory = serviceProvider.ByNameServiceFactory(types);

        var instance01 = serviceFactory(nameof(ValidService03));
        Assert.IsTrue(instance01 is ValidService03);
        
        var instance02 = serviceFactory(nameof(ValidService04));
        Assert.IsTrue(instance02 is ValidService04);
    }
}