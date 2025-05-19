using AnalysisService.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace AnalysisService.Cache
{
    public class RedisBufferService : IRedisBufferService
    {
        private readonly IDatabase _db;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public RedisBufferService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        private string GetKey(string userId) => $"analysis:buffer:{userId}";

        public async Task<AnalysisRequest?> GetRequestAsync(string userId)
        {
            var value = await _db.StringGetAsync(GetKey(userId));
            return value.HasValue ? JsonSerializer.Deserialize<AnalysisRequest>(value, _jsonOptions) : null;
        }

        public async Task SaveRequestAsync(string userId, AnalysisRequest request)
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            await _db.StringSetAsync(GetKey(userId), json, TimeSpan.FromMinutes(30));
        }

        public async Task SaveAnalysisResultAsync(string userId, AnalysisResultDto result)
        {
            var key = $"analysis:result:{userId}";
            var json = JsonSerializer.Serialize(result, _jsonOptions);
            await _db.StringSetAsync(key, json, TimeSpan.FromMinutes(60)); // TTL 24 часа
        }

        public async Task DeleteRequestAsync(string userId)
        {
            await _db.KeyDeleteAsync(GetKey(userId));
        }
    }
}
