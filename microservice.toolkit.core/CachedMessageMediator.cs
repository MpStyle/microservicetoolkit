using microservice.toolkit.core.entity;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.core;

public abstract class CachedMessageMediator(ICacheManager cacheManager) : IMessageMediator
{
    private const long CacheDuration = 1000;

    public abstract Task Init(CancellationToken cancellationToken);

    public abstract Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message,
        CancellationToken cancellationToken);

    public Task<ServiceResponse<TPayload>> Send<TPayload>(string pattern, object message)
    {
        return this.Send<TPayload>(pattern, message, CancellationToken.None);
    }

    public abstract Task Shutdown(CancellationToken cancellationToken);

    protected bool TryGetCachedResponse<TPayload>(string pattern, object message, CancellationToken cancellationToken,
        out ServiceResponse<TPayload> response)
    {
        response = default;

        return cacheManager != null &&
               cacheManager.TryGet(GetCacheKey(pattern, message), out response);
    }

    protected void SetCacheResponse<TPayload>(string pattern, object message, CancellationToken cancellationToken,
        ServiceResponse<TPayload> response)
    {
        cacheManager?.Set(GetCacheKey(pattern, message), response.Payload,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + CacheDuration);
    }

    private static string GetCacheKey(string pattern, object message)
    {
        return $"{nameof(CachedMessageMediator)}:{pattern}:{message.GetHashCode()}";
    }
}