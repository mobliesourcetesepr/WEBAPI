using StackExchange.Redis;
using System.Text.Json;
using MultiTenantAPI.Models;

namespace MultiTenantAPI.Services
{
    public class RedisSessionService
    {
        private readonly IDatabase _redis;
        
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(30);

        public RedisSessionService(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public async Task<string> CreateSessionAsync(SessionStore payload)
        {
            var token = Guid.NewGuid().ToString();
            payload.ExpiresAt = DateTime.UtcNow.Add(_sessionTimeout);
            string data = JsonSerializer.Serialize(payload);

            await _redis.StringSetAsync(token, data, _sessionTimeout);
            return token;
        }

        public async Task<SessionStore?> GetSessionAsync(string token)
        {
            var value = await _redis.StringGetAsync(token);
            if (value.IsNullOrEmpty) return null;

            var payload = JsonSerializer.Deserialize<SessionStore>(value!);
            if (payload!.ExpiresAt < DateTime.UtcNow) return null;

            // Extend session
            payload.ExpiresAt = DateTime.UtcNow.Add(_sessionTimeout);
            await _redis.StringSetAsync(token, JsonSerializer.Serialize(payload), _sessionTimeout);
            return payload;
        }

        public async Task InvalidateSessionAsync(string token)
        {
            await _redis.KeyDeleteAsync(token);
        }
    }
}
