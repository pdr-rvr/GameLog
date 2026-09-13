using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using GameLog_Backend.Services.Interfaces;

namespace GameLog_Backend.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly ConcurrentDictionary<string, (object? Value, DateTime Expiry)> _store = new();

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Task.FromResult<T?>(default);

            if (_store.TryGetValue(key, out var entry) && entry.Expiry > DateTime.UtcNow)
            {
                return Task.FromResult((T?)entry.Value);
            }

            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Task.CompletedTask;

            if (value == null)
            {
                return RemoveAsync(key, cancellationToken);
            }

            var exp = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromMinutes(10));
            _store[key] = (value, exp);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                _store.TryRemove(key, out _);
            }
            return Task.CompletedTask;
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
