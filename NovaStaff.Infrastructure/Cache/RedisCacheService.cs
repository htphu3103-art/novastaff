using System.Text.Json;
using NovaStaff.Shared.Cache;
using NovaStaff.Shared.Serialization;
using RedisDb = StackExchange.Redis.IDatabase;
using RedisMultiplexer = StackExchange.Redis.IConnectionMultiplexer;

namespace NovaStaff.Infrastructure.Cache;

public class RedisCacheService : ICacheService
{
    private readonly RedisDb _db;

    public RedisCacheService(RedisMultiplexer redis)
        => _db = redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (!value.HasValue) return default;
        return JsonSerializer.Deserialize<T>(value!, SystemJson.Default);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value, SystemJson.Default);
        await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(30));
    }

    public async Task RemoveAsync(string key)
        => await _db.KeyDeleteAsync(key);
}