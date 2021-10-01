using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book
{
    /// <summary>
    /// Common interface to manage cache using different providers.
    /// All methods return a Task (and they will be async where possible).
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// Adds an entry in the cache provider with an expiration time (Unix timestamp in milliseconds).
        /// </summary>
        /// <param name="key">Entry key</param>
        /// <param name="value">Entry value</param>
        /// <param name="issuedAt">Entry expiration time in Unix timestamp milliseconds format</param>
        /// <returns></returns>
        Task<bool> Set(string key, string value, long issuedAt);

        /// <summary>
        /// Adds an entry in the cache provider without an expiration time.
        /// </summary>
        /// <param name="key">Entry key</param>
        /// <param name="value">Entry value</param>
        /// <returns></returns>
        Task<bool> Set(string key, string value);

        /// <summary>
        /// Tries to retrieve the value of the entry using the key. If the entry does not exist or is expired the method returns null.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>A string if the entry exists end it is not expired, otherwise null</returns>
        Task<string> Get(string key);

        /// <summary>
        /// Removed the entry from the cache provider.
        /// </summary>
        /// <param name="key">The key of the entry to remove.</param>
        /// <returns></returns>
        Task<bool> Delete(string key);
    }
}