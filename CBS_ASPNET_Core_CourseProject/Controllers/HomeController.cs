using CBS_ASPNET_Core_CourseProject.Data;
using CBS_ASPNET_Core_CourseProject.Models;
using CBS_ASPNET_Core_CourseProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace CBS_ASPNET_Core_CourseProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly CurrencyService _currencyService;

        public HomeController(CurrencyService currencyService, ApplicationDbContext context)
        {
            _currencyService = currencyService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var privatBankRates = await _currencyService.GetCurrencyRatesAsync();
            var monobankRates = await _currencyService.GetMonobankCurrencyRatesAsync();

            var allRates = privatBankRates.Concat(monobankRates).ToList();

            await _currencyService.SaveCurrencyRatesAsync(allRates);

            var viewModel = new CurrencyRatesViewModel
            {
                PrivatBankRates = privatBankRates,
                MonobankRates = monobankRates
            };

            return View(viewModel);
        }

        public class CurrencyRatesViewModel
        {
            public IEnumerable<CurrencyRate> PrivatBankRates { get; set; }
            public IEnumerable<CurrencyRate> MonobankRates { get; set; }
        }

    }
}
