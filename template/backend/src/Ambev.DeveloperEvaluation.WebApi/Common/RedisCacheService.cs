using System;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Ambev.DeveloperEvaluation.WebApi.Common
{
    /// <summary>
    /// Serviço simples para leitura/escrita cache usando Redis
    /// </summary>
    public class RedisCacheService
    {
        private readonly IDatabase _cache;

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _cache = connectionMultiplexer.GetDatabase();
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var serialized = JsonSerializer.Serialize(value);
            if (expiry.HasValue)
    await _cache.StringSetAsync(key, serialized, expiry.Value);
else
    await _cache.StringSetAsync(key, serialized);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _cache.StringGetAsync(key);
            if (value.IsNullOrEmpty)
                return default;
            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.KeyDeleteAsync(key);
        }
    }
}
