using microservice.toolkit.core.attribute;
using microservice.toolkit.core.entity;

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test.data;

[ExcludeFromCodeCoverage]
[Microservice]
public class ValidService01 : ServiceAsync<int, int>
{
    public override Task<ServiceResponse<int>> RunAsync(int request, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    [Microservice]
    public class InvalidNestedService : ServiceAsync<int, int>
    {
        public override Task<ServiceResponse<int>> RunAsync(int request, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}

[ExcludeFromCodeCoverage]
[Microservice]
public class ValidService02 : ServiceAsync<int, int>
{
    public override Task<ServiceResponse<int>> RunAsync(int request, CancellationToken cancellAsyncationToken = default)
    {
        throw new System.NotImplementedException();
    }
}

[ExcludeFromCodeCoverage]
[Microservice]
public abstract class InvalidAbstractService : Service<int, int>
{
}

[ExcludeFromCodeCoverage]
[Microservice]
public class InvalidGenericService : ServiceAsync<int, int>
{
    public override Task<ServiceResponse<int>> RunAsync(int request, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}

[ExcludeFromCodeCoverage]
[Microservice(nameof(ValidService03))]
public class ValidService03 : ServiceAsync<int, int>
{
    public override Task<ServiceResponse<int>> RunAsync(int request, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}

[ExcludeFromCodeCoverage]
[Microservice(nameof(ValidService04))]
public class ValidService04 : ServiceAsync<int, int>
{
    public override Task<ServiceResponse<int>> RunAsync(int request, CancellationToken canceAsyncllationToken = default)
    {
        throw new System.NotImplementedException();
    }
}