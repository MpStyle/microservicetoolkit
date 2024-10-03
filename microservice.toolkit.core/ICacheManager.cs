using System.Threading.Tasks;

namespace microservice.toolkit.core
{
    /// <summary>
    /// Caching is a great and simple technique that helps improve your app's performance. It acts as a temporary data store providing high performance data access.
    /// ICacheManager is an interface to manage cache using different providers.
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// Removed the entry from the cache provider.
        /// </summary>
        /// <param name="key">The key of the entry to remove.</param>
        /// <returns></returns>
        bool Delete(string key);
        
        Task<bool> DeleteAsync(string key);

        /// <summary>
        /// Adds an entry in the cache provider with an expiration time (Unix timestamp in milliseconds).
        /// </summary>
        /// <typeparam name="TValue">JSON serializable object</typeparam>
        /// <param name="key">Entry key</param>
        /// <param name="value">Entry value</param>
        /// <param name="issuedAt">Entry expiration time in Unix timestamp milliseconds format</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">
        /// There is no compatible JsonConverter for TValue or its serializable members.
        /// </exception>
        bool Set<TValue>(string key, TValue value, long issuedAt);
        
        Task<bool> SetAsync<TValue>(string key, TValue value, long issuedAt);

        /// <summary>
        /// Adds an entry in the cache provider without an expiration time.
        /// </summary>
        /// <typeparam name="TValue">JSON serializable object</typeparam>
        /// <param name="key">Entry key</param>
        /// <param name="value">Entry value</param>
        /// <returns></returns>
        bool Set<TValue>(string key, TValue value);
        
        Task<bool> SetAsync<TValue>(string key, TValue value);

        /// <summary>
        /// Tries to retrieve the value of the entry using the key. If the entry does not exist or is expired the method returns null.
        /// </summary>
        /// <typeparam name="TValue">JSON serializable object</typeparam>
        /// <param name="key">Entry key</param>
        /// <returns></returns>
        TValue Get<TValue>(string key);
        
        Task<TValue> GetAsync<TValue>(string key);
        
        bool TryGet<TValue>(string key, out TValue value);
    }

    public interface ICacheValueSerializer
    {
        string Serialize<TValue>(TValue value);
        TValue Deserialize<TValue>(string value);
    }
}