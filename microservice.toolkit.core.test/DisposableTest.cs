using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.core.test;

[ExcludeFromCodeCoverage]
public class DisposableTest
{
    [Test]
    public void Dispose_CallsDisposeManageAndDisposeUnmanage()
    {
        var disposable = new DisposableTestable();

        disposable.Dispose(true);

        Assert.That(disposable.ManageDisposed, Is.True);
        Assert.That(disposable.UnmanageDisposed, Is.True);
    }

    [Test]
    public void Dispose_CalledMultipleTimes_IsIdempotent()
    {
        var disposable = new DisposableTestable();

        disposable.Dispose(false);
        disposable.ManageDisposed = false;
        disposable.UnmanageDisposed = false;

        // Second call should not re-invoke DisposeManage/DisposeUnmanage
        disposable.Dispose(false);

        Assert.That(disposable.ManageDisposed, Is.False);
        Assert.That(disposable.UnmanageDisposed, Is.False);
    }

    [Test]
    public void Finalizer_CallsDisposeUnmanageOnly()
    {
        var disposable = new DisposableTestable();
        // Simulate finalizer by calling Dispose(false)
        disposable.Dispose(false);

        Assert.That(disposable.ManageDisposed, Is.False);
        Assert.That(disposable.UnmanageDisposed, Is.True);
    }
}


public class DisposableTestable : Disposable
{
    public bool ManageDisposed { get; set; }
    public bool UnmanageDisposed { get; set; }

    protected override void DisposeManage()
    {
        this.ManageDisposed = true;
    }

    protected override void DisposeUnmanage()
    {
        this.UnmanageDisposed = true;
    }
}