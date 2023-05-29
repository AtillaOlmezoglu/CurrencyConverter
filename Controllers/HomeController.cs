using CurrencyConverter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Text.Json;

namespace CurrencyConverter.Controllers;

public class HomeController : Controller
{
    private readonly HelperMethods _helperMethods;
    private readonly IMemoryCache _cache;
    public HomeController(HelperMethods helperMethods, IMemoryCache cache)
    {
        _helperMethods = helperMethods;
        _cache = cache;
    }


    public IActionResult Index(decimal amount, string searchString)
    {
        var exchangeRates = GetDataFromCache().GetAwaiter().GetResult();

        var deserializedExchangeRates = JsonSerializer.Deserialize<JsonDocument>(exchangeRates);

        var ratesObject = deserializedExchangeRates.RootElement.GetProperty("rates");

        var convertedIndex = new Dictionary<string, decimal>();

        foreach (var rate in ratesObject.EnumerateObject())
        {
            var currencyCode = rate.Name;
            var currencyValue = rate.Value.GetDecimal() * amount;

            convertedIndex[currencyCode] = currencyValue;
        }

        List<JsonProperty> searchResults = new List<JsonProperty>();

        if (!String.IsNullOrEmpty(searchString))
        {
            searchResults = ratesObject
                .EnumerateObject()
                .Where(exchangeCountry =>
                    exchangeCountry.Name
                    .Contains(searchString, StringComparison.OrdinalIgnoreCase))
                .ToList();

            convertedIndex = searchResults.ToDictionary(result => 
                result.Name,
                result => result.Value.GetDecimal() * amount
            );
        }

        ViewBag.SearchString = searchString;

        return View(convertedIndex);
    }
    public async Task<string> GetDataFromCache()
    {
        await _helperMethods.UpdateCache();

        var cachedData = _cache.Get<string>("CachedApiData");

        if (cachedData != null)
        {
            return cachedData;
        };

        throw new Exception("Data not found.");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}