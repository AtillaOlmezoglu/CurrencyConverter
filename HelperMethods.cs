using Microsoft.Extensions.Caching.Memory;

namespace CurrencyConverter;

public class HelperMethods
{
    private readonly HttpClient _httpClient;
    private string? _apiKey;
    private readonly IMemoryCache _cache;

    public HelperMethods(HttpClient httpClient, IConfiguration config, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _apiKey = config["Api:Key"];
        _cache = cache;
    }

    public async Task UpdateCache()
    {
        var cachedData = _cache.Get<string>("CachedApiData");

        if (cachedData == null)
        {
            var response = await _httpClient.GetAsync($"https://openexchangerates.org/api/latest.json?app_id={_apiKey}");
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsStringAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(2));

            _cache.Set("CachedApiData", data, cacheEntryOptions);
        }
    }
}
