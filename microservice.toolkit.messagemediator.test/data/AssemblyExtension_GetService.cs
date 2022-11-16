using microservice.toolkit.core.entity;
using microservice.toolkit.messagemediator.attribute;

using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test.data
{
    [Microservice]
    public class ValidService01 : Service<int, int>
    {
        public override Task<ServiceResponse<int>> Run(int request)
        {
            throw new System.NotImplementedException();
        }

        [Microservice]
        public class InvalidNestedService : Service<int, int>
        {
            public override Task<ServiceResponse<int>> Run(int request)
            {
                throw new System.NotImplementedException();
            }
        }
    }

    [Microservice]
    public class ValidService02 : Service<int, int>
    {
        public override Task<ServiceResponse<int>> Run(int request)
        {
            throw new System.NotImplementedException();
        }
    }

    [Microservice]
    public abstract class InvalidAbstractService : Service<int, int>
    {
    }

    [Microservice]
    public class InvalidGenericService<T> : Service<int, int>
    {
        public override Task<ServiceResponse<int>> Run(int request)
        {
            throw new System.NotImplementedException();
        }
    }

    [Microservice(nameof(ValidService03))]
    public class ValidService03 : Service<int, int>
    {
        public override Task<ServiceResponse<int>> Run(int request)
        {
            throw new System.NotImplementedException();
        }
    }

    [Microservice(nameof(ValidService04))]
    public class ValidService04 : Service<int, int>
    {
        public override Task<ServiceResponse<int>> Run(int request)
        {
            throw new System.NotImplementedException();
        }
    }
}