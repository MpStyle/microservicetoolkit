using microservice.toolkit.core.entity;
using microservice.toolkit.messagemediator.attribute;

using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test.data
{
    [MicroService]
    public class ValidService01 : Service<int, int>
    {
        public override Task<ServiceResponse<int>> Run(int request)
        {
            throw new System.NotImplementedException();
        }

        [MicroService]
        public class InvalidNestedService : Service<int, int>
        {
            public override Task<ServiceResponse<int>> Run(int request)
            {
                throw new System.NotImplementedException();
            }
        }
    }

    [MicroService]
    public class ValidService02 : Service<int, int>
    {
        public override Task<ServiceResponse<int>> Run(int request)
        {
            throw new System.NotImplementedException();
        }
    }

    [MicroService]
    public abstract class InvalidAbstractService : Service<int, int>
    {
    }

    [MicroService]
    public class InvalidGenericService<T> : Service<int, int>
    {
        public override Task<ServiceResponse<int>> Run(int request)
        {
            throw new System.NotImplementedException();
        }
    }

    [MicroService(nameof(ValidService03))]
    public class ValidService03 : Service<int, int>
    {
        public override Task<ServiceResponse<int>> Run(int request)
        {
            throw new System.NotImplementedException();
        }
    }

    [MicroService(nameof(ValidService04))]
    public class ValidService04 : Service<int, int>
    {
        public override Task<ServiceResponse<int>> Run(int request)
        {
            throw new System.NotImplementedException();
        }
    }
}