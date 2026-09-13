using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GameLog_Backend.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace GameLog_Backend.Services
{
    public class DistributedCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<DistributedCacheService>? _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public DistributedCacheService(
            IDistributedCache cache, 
            ILogger<DistributedCacheService>? logger = null)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                return default;

            try
            {
                var json = await _cache.GetStringAsync(key, cancellationToken);
                if (string.IsNullOrWhiteSpace(json))
                    return default;

                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "[Cache] Falha ao recuperar chave '{Key}' do cache distribuído.", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            if (value == null)
            {
                await RemoveAsync(key, cancellationToken);
                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(value, _jsonOptions);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
                };

                await _cache.SetStringAsync(key, json, options, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "[Cache] Falha ao gravar chave '{Key}' no cache distribuído.", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            try
            {
                await _cache.RemoveAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "[Cache] Falha ao remover chave '{Key}' do cache distribuído.", key);
            }
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var cached = await GetAsync<T>(key, cancellationToken);
            if (cached != null)
            {
                return cached;
            }

            var item = await factory();
            if (item != null)
            {
                await SetAsync(key, item, expiration, cancellationToken);
            }

            return item;
        }
    }
}
