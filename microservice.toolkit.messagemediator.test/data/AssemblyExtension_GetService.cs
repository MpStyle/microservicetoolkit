using microservice.toolkit.core.entity;

using System.Threading.Tasks;

namespace microservice.toolkit.messagemediator.test.data
{
    public class ValidService01:Service<int,int>
    {
        public override Task<ServiceResponse<int>> Run(int request)
        {
            throw new System.NotImplementedException();
        }
    
        public class InvalidNestedService:Service<int,int>
        {
            public override Task<ServiceResponse<int>> Run(int request)
            {
                throw new System.NotImplementedException();
            }
        }
    }

    public class ValidService02 : Service<int, int>
    {
        public override Task<ServiceResponse<int>> Run(int request)
        {
            throw new System.NotImplementedException();
        }
    }

    public abstract class InvalidAbstractService : Service<int, int>
    {
    }
    
    public class InvalidGenericService<T> : Service<int, int>
    {
        public override Task<ServiceResponse<int>> Run(int request)
        {
            throw new System.NotImplementedException();
        }
    }
}