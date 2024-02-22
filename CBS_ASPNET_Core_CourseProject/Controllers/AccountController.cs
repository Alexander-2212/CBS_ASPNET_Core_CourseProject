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
                await _emailService.SendEmailAsync(user.Email, "Підсумок ваших щоденних курсів валют", summaryHtml);

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
            builder.Append("<h1>Сьогоднішні курси валют</h1>");
            builder.Append("<h2>Курси ПриватБанку</h2>");
            builder.Append("<table><thead><tr><th>Валюта</th><th>Купівля</th><th>Продаж</th></tr></thead><tbody>");

            foreach (var rate in rates)
            {
                builder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", rate.CurrencyCode, rate.BuyRate, rate.SellRate);
            }

            builder.Append("</tbody></table>");

            builder.Append("<h2>Курси Монобанку</h2>");
            builder.Append("<table><thead><tr><th>Валюта</th><th>Купівля</th><th>Продаж</th></tr></thead><tbody>");

            foreach (var rate in monoRates)
            {
                builder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", rate.CurrencyCode, rate.BuyRate, rate.SellRate);
            }

            builder.Append("</tbody></table>");

            builder.Append("<h2>Курси НБУ</h2>");
            builder.Append("<table><thead><tr><th>Валюта</th><th>Курс</th></tr></thead><tbody>");

            foreach (var rate in NBURates)
            {
                builder.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", rate.CurrencyCode, rate.BuyRate);
            }

            builder.Append("</tbody></table>");

            return builder.ToString();
        }



        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AccountSettings()
        {
            var user = await _userManager.GetUserAsync(User);
            var model = new AccountSettingsViewModel
            {
                Email = user.Email,
                WantsEmailNotifications = user.WantsEmailNotifications
            };
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AccountSettings(AccountSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                user.WantsEmailNotifications = model.WantsEmailNotifications;

                IdentityResult result = IdentityResult.Success;

                if (!string.IsNullOrEmpty(model.NewPassword) && !string.IsNullOrEmpty(model.CurrentPassword))
                {
                    result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                }
                else
                {
                    result = await _userManager.UpdateAsync(user);
                }

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Налаштування облікового запису успішно оновлено.";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }



    }
}
