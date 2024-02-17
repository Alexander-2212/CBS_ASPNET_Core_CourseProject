using CBS_ASPNET_Core_CourseProject.Data;
using CBS_ASPNET_Core_CourseProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace CBS_ASPNET_Core_CourseProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly CurrencyService _currencyService;
        private readonly ApplicationDbContext _context;

        public HomeController(CurrencyService currencyService, ApplicationDbContext context)
        {
            _currencyService = currencyService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rates = await _currencyService.GetCurrencyRatesAsync();
            foreach (var rate in rates)
            {
                _context.CurrencyRates.Add(rate);
            }
            await _context.SaveChangesAsync();

            return View(rates);
        }
    }
}
