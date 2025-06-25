using microservice.toolkit.messagemediator.extension;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test.extension;

[ExcludeFromCodeCoverage]
public class SignalEmitterExtensionsTest
{

    [Test]
    public async Task Emit_UsesPatternFromType()
    {
        var emitter = new DummySignalEmitter();
        var message = "test-message";
        var type = typeof(DummyHandler);

        await emitter.Emit(type, message);

        Assert.That(emitter.LastPattern, Is.EqualTo(type.ToPattern()));
        Assert.That(emitter.LastMessage, Is.EqualTo(message));
    }

    [Test]
    public void GetHandlers_ByType()
    {
        var handlers = typeof(DummyHandler).GetHandlers();

        Assert.That(handlers.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(handlers.ContainsPattern(typeof(DummyHandler).ToPattern()), Is.True);
        Assert.That(handlers[typeof(DummyHandler).ToPattern()].First(), Is.EqualTo(typeof(DummyHandler)));
    }

    [Test]
    public void GetHandlers_ByTypes()
    {
        var types = new[] { typeof(DummyHandler) };
        var handlers = types.GetHandlers();

        Assert.That(handlers.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(handlers.ContainsPattern(typeof(DummyHandler).ToPattern()), Is.True);
        Assert.That(handlers[typeof(DummyHandler).ToPattern()].First(), Is.EqualTo(typeof(DummyHandler)));
    }

    [Test]
    public void GetHandlers_ByAssembly()
    {
        var assembly = Assembly.GetAssembly(typeof(DummyHandler));
        var handlers = assembly.GetHandlers();

        Assert.That(handlers.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(handlers.ContainsPattern(typeof(DummyHandler).ToPattern()), Is.True);
        Assert.That(handlers[typeof(DummyHandler).ToPattern()].First(), Is.EqualTo(typeof(DummyHandler)));
    }

    [Test]
    public void GetHandlers_ByAssemblies()
    {
        var assemblies = new[] { Assembly.GetAssembly(typeof(DummyHandler)) };
        var handlers = assemblies.GetHandlers();

        Assert.That(handlers.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(handlers.ContainsPattern(typeof(DummyHandler).ToPattern()), Is.True);
        Assert.That(handlers[typeof(DummyHandler).ToPattern()].First(), Is.EqualTo(typeof(DummyHandler)));
    }

    [Test]
    public void AddHandlerContext_ByType_Singleton()
    {
        var services = new ServiceCollection()
            .AddHandlerContext(typeof(DummyHandler))
            .BuildServiceProvider();

        var instance1 = services.GetService<DummyHandler>();
        var instance2 = services.GetService<DummyHandler>();

        Assert.That(instance1, Is.EqualTo(instance2));
        Assert.That(instance1, Is.SameAs(instance2));
    }

    [Test]
    public void AddHandlerContext_ByType_Transient()
    {
        var services = new ServiceCollection()
            .AddHandlerContext(typeof(DummyHandler), ServiceLifetime.Transient)
            .BuildServiceProvider();

        var instance1 = services.GetService<DummyHandler>();
        var instance2 = services.GetService<DummyHandler>();

        Assert.That(instance1, Is.Not.EqualTo(instance2));
        Assert.That(instance1, Is.Not.SameAs(instance2));
    }

    [Test]
    public void AddHandlerContext_ByTypes()
    {
        var types = new[] { typeof(DummyHandler) };
        var services = new ServiceCollection()
            .AddHandlerContext(types)
            .BuildServiceProvider();

        var instance = services.GetService<DummyHandler>();
        Assert.That(instance, Is.Not.Null);
    }

    [Test]
    public void AddHandlerContext_ByAssembly()
    {
        var assembly = Assembly.GetAssembly(typeof(DummyHandler));
        var services = new ServiceCollection()
            .AddHandlerContext(assembly)
            .BuildServiceProvider();

        var instance = services.GetService<DummyHandler>();
        Assert.That(instance, Is.Not.Null);
    }

    [Test]
    public void AddHandlerContext_ByAssemblies()
    {
        var assemblies = new[] { typeof(DummyHandler).Assembly };
        var services = new ServiceCollection()
            .AddHandlerContext(assemblies)
            .BuildServiceProvider();

        var instance = services.GetService<DummyHandler>();
        Assert.That(instance, Is.Not.Null);
    }
}

[ExcludeFromCodeCoverage]
class DummySignalEmitter : ISignalEmitter
{
    public string? LastPattern { get; private set; }
    public object? LastMessage { get; private set; }

    public Task Init(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task Emit<TEvent>(string pattern, TEvent message, CancellationToken cancellationToken = default)
    {
        this.LastPattern = pattern;
        this.LastMessage = message;
        return Task.CompletedTask;
    }

    public Task Shutdown(CancellationToken cancellationToken) => Task.CompletedTask;
}

[ExcludeFromCodeCoverage]
public class DummyHandler : SignalHandler<string>
{
    public override Task Run(string request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}