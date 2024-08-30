using microservice.toolkit.core.entity;

using System;
using System.Threading.Tasks;

namespace microservice.toolkit.core;

public abstract class CachedMessageMediator : IMessageMediator
{
    private const long CacheDuration = 1000;
    private readonly ICacheManager cacheManager;

    protected CachedMessageMediator(ICacheManager cacheManager)
    {
        this.cacheManager = cacheManager;
    }

    public abstract Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message);
    public abstract Task Shutdown();

    protected bool TryGetCachedResponse<TPayload>(string pattern, object message,
        out ServiceResponse<TPayload> response)
    {
        response = default;

        return this.cacheManager != null &&
               this.cacheManager.TryGet(GetCacheKey(pattern, message), out response);
    }

    protected void SetCacheResponse<TPayload>(string pattern, object message, ServiceResponse<TPayload> response)
    {
        this.cacheManager?.Set(GetCacheKey(pattern, message), response.Payload,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + CacheDuration);
    }

    private static string GetCacheKey(string pattern, object message)
    {
        return $"{nameof(CachedMessageMediator)}:{pattern}:{message.GetHashCode()}";
    }
}