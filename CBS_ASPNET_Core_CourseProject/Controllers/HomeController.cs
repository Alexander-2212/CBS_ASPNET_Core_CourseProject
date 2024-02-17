using CBS_ASPNET_Core_CourseProject.Data;
using CBS_ASPNET_Core_CourseProject.Models;
using CBS_ASPNET_Core_CourseProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace CBS_ASPNET_Core_CourseProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly CurrencyService _currencyService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(CurrencyService currencyService, ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Fetching currency rates for PrivatBank and Monobank");

            var privatBankRates = await _currencyService.GetCurrencyRatesAsync();
            var monobankRates = await _currencyService.GetMonobankCurrencyRatesAsync();

            if (!privatBankRates.Any() || !monobankRates.Any())
            {
                _logger.LogWarning("One or both of the banks did not return any currency rates.");
            }

            var allRates = privatBankRates.Concat(monobankRates).ToList();

            await _currencyService.SaveCurrencyRatesAsync(allRates);

            var viewModel = new CurrencyRatesViewModel
            {
                PrivatBankRates = privatBankRates,
                MonobankRates = monobankRates
            };

            _logger.LogInformation("Currency rates successfully fetched and saved.");

            return View(viewModel);
        }

        public class CurrencyRatesViewModel
        {
            public IEnumerable<CurrencyRate> PrivatBankRates { get; set; }
            public IEnumerable<CurrencyRate> MonobankRates { get; set; }
        }

    }
}
