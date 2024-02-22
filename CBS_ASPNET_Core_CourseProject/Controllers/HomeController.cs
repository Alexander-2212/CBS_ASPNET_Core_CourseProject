using CBS_ASPNET_Core_CourseProject.Data;
using CBS_ASPNET_Core_CourseProject.Entities;
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
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> CurrencyRates()
        {
            _logger.LogInformation("Отримання курсів валют для ПриватБанку та Монобанку");

            var privatBankRates = await _currencyService.GetCurrencyRatesAsync();
            var monobankRates = await _currencyService.GetMonobankCurrencyRatesAsync();
            var nbuRates = await _currencyService.GetNbuCurrencyRatesAsync();

            if (!privatBankRates.Any() || !monobankRates.Any() || !nbuRates.Any())
            {
                _logger.LogWarning("Один або декілька банків не повернули курси валют.");
            }

            var allRates = privatBankRates.Concat(monobankRates).Concat(nbuRates).ToList();

            await _currencyService.SaveCurrencyRatesAsync(allRates);

            var viewModel = new CurrencyRatesViewModel
            {
                PrivatBankRates = privatBankRates,
                MonobankRates = monobankRates,
                NbuRates = nbuRates
            };

            _logger.LogInformation("Курси валют успішно отримано та збережено.");

            return View(viewModel);
        }
    }
}
