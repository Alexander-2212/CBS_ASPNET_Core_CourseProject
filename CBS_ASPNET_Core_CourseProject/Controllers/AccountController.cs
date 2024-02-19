using CBS_ASPNET_Core_CourseProject.Controllers.Extensions;
using CBS_ASPNET_Core_CourseProject.Entities;
using CBS_ASPNET_Core_CourseProject.Models;
using CBS_ASPNET_Core_CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace CBS_ASPNET_Core_CourseProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        private readonly CurrencyService _currencyRateService;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IEmailService emailService, CurrencyService currencyRateService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _currencyRateService = currencyRateService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User { UserName = model.Email, Email = model.Email, WantsEmailNotifications = model.WantsEmailNotifications };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("index", "home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [Authorize]
        public async Task<IActionResult> SendCurrencyRatesEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && user.WantsEmailNotifications)
            {
                var summaryHtml = await GetCurrencyRatesSummaryAsync();
                await _emailService.SendEmailAsync(user.Email, "Your Daily Currency Rates Summary", summaryHtml);

                return RedirectToAction("Index", "Home").WithSuccess("Success", "Email sent successfully.");
            }
            return RedirectToAction("Index", "Home").WithError("Error", "Unable to send email.");
        }

        private async Task<string> GetCurrencyRatesSummaryAsync()
        {
            var rates = await _currencyRateService.GetCurrencyRatesAsync();
            var monoRates = await _currencyRateService.GetMonobankCurrencyRatesAsync();
            var NBURates = await _currencyRateService.GetNbuCurrencyRatesAsync();

            var builder = new StringBuilder();
            builder.Append("<h1>Daily Currency Rates Summary</h1>");
            builder.Append("<h2>Privat Rates</h2>");
            builder.Append("<table><thead><tr><th>Currency</th><th>Buy Rate</th><th>Sell Rate</th></tr></thead><tbody>");

            foreach (var rate in rates)
            {
                builder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", rate.CurrencyCode, rate.BuyRate, rate.SellRate);
            }

            builder.Append("</tbody></table>");

            builder.Append("<h2>Monobank Rates</h2>");
            builder.Append("<table><thead><tr><th>Currency</th><th>Buy Rate</th><th>Sell Rate</th></tr></thead><tbody>");


            foreach (var rate in monoRates)
            {
                builder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", rate.CurrencyCode, rate.BuyRate, rate.SellRate);
            }

            builder.Append("</tbody></table>");

            builder.Append("<h2>NBU Rates</h2>");
            builder.Append("<table><thead><tr><th>Currency</th><th>Buy Rate</th><th>Sell Rate</th></tr></thead><tbody>");


            foreach (var rate in NBURates)
            {
                builder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", rate.CurrencyCode, rate.BuyRate, rate.SellRate);
            }

            builder.Append("</tbody></table>");

            return builder.ToString();
        }


    }
}
