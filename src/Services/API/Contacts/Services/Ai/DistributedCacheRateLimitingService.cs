using API.Contacts.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace API.Contacts.Services;

/// <summary>
/// Implementation of the rate limiting service using a distributed cache
/// </summary>
public class DistributedCacheRateLimitingService : IRateLimitingService
{
    private readonly IDistributedCache _cache;
    private readonly AiAssistantOptions _options;

    public DistributedCacheRateLimitingService(
        IDistributedCache cache,
        IOptions<AiAssistantOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public async Task<bool> AllowRequestAsync(string userId, string operationType)
    {
        var cacheKey = $"rate-limit:{operationType}:{userId}";
        var countBytes = await _cache.GetAsync(cacheKey);

        if (countBytes == null)
        {
            // First request in the time window
            await _cache.SetAsync(
                cacheKey,
                BitConverter.GetBytes(1),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.RequestLimitCooldownMinutes)
                });
            
            return true;
        }

        var count = BitConverter.ToInt32(countBytes);
        
        if (count >= _options.RequestLimitPerMinute)
        {
            // Rate limit exceeded
            return false;
        }

        // Increment the counter
        await _cache.SetAsync(
            cacheKey,
            BitConverter.GetBytes(count + 1),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.RequestLimitCooldownMinutes)
            });
        
        return true;
    }

    public async Task<TimeSpan> GetTimeUntilNextAllowedRequestAsync(string userId, string operationType)
    {
        var cacheKey = $"rate-limit:{operationType}:{userId}";
        var countBytes = await _cache.GetAsync(cacheKey);

        if (countBytes == null)
        {
            // No rate limit currently active
            return TimeSpan.Zero;
        }

        var count = BitConverter.ToInt32(countBytes);
        
        if (count < _options.RequestLimitPerMinute)
        {
            // Requests are still allowed
            return TimeSpan.Zero;
        }

        // Get the cache entry expiration
        // In a real implementation, we would store the expiration time in the cache
        // This is a simplified version that always returns the configured cooldown time
        return TimeSpan.FromMinutes(_options.RequestLimitCooldownMinutes);
    }
}
