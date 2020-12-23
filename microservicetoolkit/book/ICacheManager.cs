using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book
{
    public interface ICacheManager
    {
        Task<bool> Set(string key, string value, long issuedAt);
        Task<string> Get(string key);
    }
}