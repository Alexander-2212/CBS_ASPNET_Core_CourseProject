using CBS_ASPNET_Core_CourseProject.Data;
using CBS_ASPNET_Core_CourseProject.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CBS_ASPNET_Core_CourseProject.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace CBS_ASPNET_Core_CourseProject.Services
{
    public class CurrencyService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public CurrencyService(ApplicationDbContext context, HttpClient httpClient, IMemoryCache cache)
        {
            _context = context;
            _httpClient = httpClient;
            _cache = cache;
        }
        public async Task<IEnumerable<CurrencyRate>> GetCurrencyRatesAsync()
        {
            const string cacheKey = "PrivatBankCurrencyRates";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<CurrencyRate> cachedRates))
            {
                var responseString = await _httpClient.GetStringAsync("https://api.privatbank.ua/p24api/pubinfo?exchange&json&coursid=11");
                var privatBankRates = JsonSerializer.Deserialize<List<PrivatBankCurrencyRate>>(responseString);

                var currencyRates = privatBankRates.Select(rate => new CurrencyRate
                {
                    CurrencyCode = rate.ccy,
                    BaseCurrencyCode = rate.base_ccy,
                    BuyRate = decimal.Parse(rate.buy),
                    SellRate = decimal.Parse(rate.sale),
                    Source = "PrivatBank"
                }).ToList();

                _cache.Set(cacheKey, currencyRates, TimeSpan.FromMinutes(1));
                return currencyRates;
            }

            return cachedRates;
        }

        public async Task<IEnumerable<CurrencyRate>> GetMonobankCurrencyRatesAsync()
        {
            const string cacheKey = "MonobankCurrencyRates";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<CurrencyRate> cachedRates))
            {
                var responseString = await _httpClient.GetStringAsync("https://api.monobank.ua/bank/currency");
                var monobankRates = JsonSerializer.Deserialize<List<MonobankCurrencyRate>>(responseString);

                var currencyCodesOfInterest = new Dictionary<int, string>
                {
                    { 840, "USD" },
                    { 978, "EUR" }
                };
                int baseCurrencyCodeUAH = 980;

                var filteredRates = monobankRates
                    .Where(rate => currencyCodesOfInterest.ContainsKey(rate.currencyCodeA) && rate.currencyCodeB == baseCurrencyCodeUAH)
                    .Select(rate => new CurrencyRate
                    {
                        CurrencyCode = currencyCodesOfInterest[rate.currencyCodeA],
                        BaseCurrencyCode = "UAH",
                        BuyRate = rate.rateBuy,
                        SellRate = rate.rateSell,
                        Source = "Monobank"
                    });

                _cache.Set(cacheKey, filteredRates, TimeSpan.FromMinutes(1));
                return filteredRates;
            }

            return cachedRates;
        }

        public async Task<IEnumerable<CurrencyRate>> GetNbuCurrencyRatesAsync()
        {
            const string cacheKey = "NbuCurrencyRates";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<CurrencyRate> cachedRates))
            {
                var responseString = await _httpClient.GetStringAsync("https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json");
                var nbuRates = JsonSerializer.Deserialize<List<NBUCurrencyRate>>(responseString);

                var filteredRates = nbuRates.Where(rate => rate.cc == "USD" || rate.cc == "EUR")
                    .Select(rate => new CurrencyRate
                    {
                        CurrencyCode = rate.cc,
                        BaseCurrencyCode = "UAH",
                        BuyRate = rate.rate,
                        SellRate = rate.rate,
                        Source = "NBU"
                    }).ToList();

                _cache.Set(cacheKey, filteredRates, TimeSpan.FromMinutes(1));
                return filteredRates;
            }

            return cachedRates;
        }

        public async Task SaveCurrencyRatesAsync(IEnumerable<CurrencyRate> rates)
        {
            foreach (var rate in rates)
            {
                var existingRate = _context.CurrencyRates.FirstOrDefault(cr => cr.CurrencyCode == rate.CurrencyCode && cr.Source == rate.Source);
                if (existingRate != null)
                {
                    existingRate.BuyRate = rate.BuyRate;
                    existingRate.SellRate = rate.SellRate;
                }
                else
                {
                    _context.CurrencyRates.Add(rate);
                }
            }
            await _context.SaveChangesAsync();
        }

    }
}
