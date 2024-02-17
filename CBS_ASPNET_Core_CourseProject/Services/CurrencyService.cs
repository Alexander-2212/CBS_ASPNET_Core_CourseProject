using CBS_ASPNET_Core_CourseProject.Data;
using CBS_ASPNET_Core_CourseProject.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CBS_ASPNET_Core_CourseProject.Services
{
    public class CurrencyService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public CurrencyService(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }
        public async Task<IEnumerable<CurrencyRate>> GetCurrencyRatesAsync()
        {
            var responseString = await _httpClient.GetStringAsync("https://api.privatbank.ua/p24api/pubinfo?exchange&json&coursid=11");
            var privatBankRates = JsonSerializer.Deserialize<List<PrivatBankCurrencyRate>>(responseString);

            var currencyRates = new List<CurrencyRate>();

            foreach (var rate in privatBankRates)
            {
                currencyRates.Add(new CurrencyRate
                {
                    CurrencyCode = rate.ccy,
                    BaseCurrencyCode = rate.base_ccy,
                    BuyRate = decimal.Parse(rate.buy),
                    SellRate = decimal.Parse(rate.sale),
                    Source = "PrivatBank"
                });
            }

            return currencyRates;
        }

        public async Task<IEnumerable<CurrencyRate>> GetMonobankCurrencyRatesAsync()
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

            return filteredRates;
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
