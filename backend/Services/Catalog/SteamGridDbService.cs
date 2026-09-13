using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GameLog_Backend.Services.Interfaces;

namespace GameLog_Backend.Services.Catalog
{
    public class SgdbAutocompleteResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public List<SgdbGameItem>? Data { get; set; }
    }

    public class SgdbGameItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("release_date")]
        public long? ReleaseDate { get; set; }
    }

    public class SgdbGridResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public List<SgdbGridItem>? Data { get; set; }
    }

    public class SgdbGridItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("thumb")]
        public string Thumb { get; set; } = string.Empty;

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }

    public class SteamGridDbService
    {
        private readonly HttpClient _httpClient;
        private readonly ICacheService _cache;
        private readonly ILogger<SteamGridDbService> _logger;
        private readonly string? _apiKey;

        [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
        public SteamGridDbService(
            HttpClient httpClient,
            ICacheService cache,
            IConfiguration configuration,
            ILogger<SteamGridDbService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;

            _apiKey = Environment.GetEnvironmentVariable("STEAMGRIDDB_API_KEY") 
                ?? configuration["SteamGridDb:ApiKey"];

            _httpClient.BaseAddress = new Uri("https://www.steamgriddb.com/api/v2/");
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GameLog", "1.0"));
            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey.Trim());
            }
        }

        public async Task<string?> ObterCapaVertical600x900Async(string tituloJogo, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tituloJogo) || string.IsNullOrWhiteSpace(_apiKey))
            {
                return null;
            }

            var cacheKey = $"sgdb_cover_{tituloJogo.Trim().ToLowerInvariant()}";
            var cached = await _cache.GetAsync<string>(cacheKey, cancellationToken);
            if (!string.IsNullOrWhiteSpace(cached))
            {
                return cached;
            }

            try
            {
                // 1. Autocomplete Search
                var searchUrl = $"search/autocomplete/{Uri.EscapeDataString(tituloJogo.Trim())}";
                var searchRes = await _httpClient.GetFromJsonAsync<SgdbAutocompleteResponse>(searchUrl, cancellationToken);
                if (searchRes == null || !searchRes.Success || searchRes.Data == null || searchRes.Data.Count == 0)
                {
                    return null;
                }

                var gameId = searchRes.Data[0].Id;

                // 2. Fetch 600x900 Grid
                var gridUrl = $"grids/game/{gameId}?dimensions=600x900&types=static";
                var gridRes = await _httpClient.GetFromJsonAsync<SgdbGridResponse>(gridUrl, cancellationToken);
                if (gridRes != null && gridRes.Success && gridRes.Data != null && gridRes.Data.Count > 0)
                {
                    var imageUrl = gridRes.Data[0].Url;
                    if (!string.IsNullOrWhiteSpace(imageUrl))
                    {
                        await _cache.SetAsync(cacheKey, imageUrl, TimeSpan.FromDays(7), cancellationToken);
                        return imageUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[SteamGridDB] Não foi possível obter capa vertical para '{Titulo}'", tituloJogo);
            }

            return null;
        }
    }
}
