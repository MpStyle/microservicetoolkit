using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book
{
    public interface IConnectionManager
    {
        T Execute<T>(Func<DbCommand, T> lambda);
        Task<T> ExecuteAsync<T>(Func<DbCommand, Task<T>> lambda);
        DbParameter GetParameter<T>(string name, T value);
    }
}
