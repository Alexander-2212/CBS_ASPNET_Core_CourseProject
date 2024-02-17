using CBS_ASPNET_Core_CourseProject.Data;
using CBS_ASPNET_Core_CourseProject.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CBS_ASPNET_Core_CourseProject.Services
{
    public class CurrencyService
    {
        private readonly HttpClient _httpClient;

        public CurrencyService(HttpClient httpClient)
        {
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
                    SellRate = decimal.Parse(rate.sale)
                });
            }

            return currencyRates;
        }

    }
}
