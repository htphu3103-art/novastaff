using NovaStaff.BusinessLayers.Interfaces.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.Infrastructure.Token
{
    public class RedisTokenBlacklistService : ITokenBlacklistService
    {
        private readonly IDatabase _redis;

        public RedisTokenBlacklistService(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public async Task BlacklistAsync(string jti, TimeSpan ttl, CancellationToken ct = default)
        {
            await _redis.StringSetAsync(
                key: $"blacklist:{jti}",
                value: "1",
                expiry: ttl);
        }

        public async Task<bool> IsBlacklistedAsync(string jti, CancellationToken ct = default)
        {
            return await _redis.KeyExistsAsync($"blacklist:{jti}");
        }
        public async Task<bool> IsUserBlacklistedAsync(int userId, CancellationToken ct = default)
        {
            return await _redis.KeyExistsAsync($"blacklist:user:{userId}");
        }
        public async Task BlacklistUserAsync(int userId, TimeSpan ttl, CancellationToken ct = default)
        {
            await _redis.StringSetAsync(
                key: $"blacklist:user:{userId}",
                value: "1",
                expiry: ttl);
        }
    }
}
