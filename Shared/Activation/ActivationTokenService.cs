using NovaStaff.Shared.Activation;
using NovaStaff.Shared.Serialization;
using System.Text.Json;
using StackExchange.Redis;  // ← thêm dòng này

namespace NovaStaff.Infrastructure.Activation;

public class ActivationTokenService : IActivationTokenService
{
    private readonly IDatabase _db;  // ← bỏ StackExchange.Redis. ở đầu
    private const string Prefix = "activation:";
    private static readonly TimeSpan Expiry = TimeSpan.FromHours(48);

    public ActivationTokenService(IConnectionMultiplexer redis)  // ← bỏ RedisMultiplexer alias
        => _db = redis.GetDatabase();

    public async Task<string> CreateAsync(ActivationTokenData data, CancellationToken ct = default)
    {
        var token = Guid.NewGuid().ToString("N");
        var key = $"{Prefix}{token}";
        var json = JsonSerializer.Serialize(data, SystemJson.Default);
        await _db.StringSetAsync(key, json, Expiry);
        return token;
    }

    public async Task<ActivationTokenData?> GetAsync(string token, CancellationToken ct = default)
    {
        var key = $"{Prefix}{token}";
        var value = await _db.StringGetAsync(key);
        if (!value.HasValue) return null;
        return JsonSerializer.Deserialize<ActivationTokenData>(value!, SystemJson.Default);
    }

    public async Task RevokeAsync(string token, CancellationToken ct = default)
    {
        var key = $"{Prefix}{token}";
        await _db.KeyDeleteAsync(key);
    }
}