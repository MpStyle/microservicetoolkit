using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book
{
    public interface ICacheManager
    {
        Task<bool> Set(string key, string value, long issuedAt);
        Task<bool> Set(string key, string value);
        Task<string> Get(string key);
        Task<bool> Delete(string key);
    }
}