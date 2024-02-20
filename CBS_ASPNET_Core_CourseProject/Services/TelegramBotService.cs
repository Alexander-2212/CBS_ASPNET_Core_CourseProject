using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static System.Formats.Asn1.AsnWriter;

namespace CBS_ASPNET_Core_CourseProject.Services
{
    public class TelegramBotService : BackgroundService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramBotService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;

        public TelegramBotService(ITelegramBotClient botClient, ILogger<TelegramBotService> logger, IServiceScopeFactory scopeFactory, IMemoryCache cache)
        {
            _botClient = botClient;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _cache = cache;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var me = await _botClient.GetMeAsync(stoppingToken);
            Debug.WriteLine($"Bot {me.Username} is ready to receive messages...");

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };
            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                stoppingToken
            );

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Text != null && update.Message.Text.ToLower().Contains("/rates"))
            {
                _logger.LogInformation("Отримано команду /rates, готуємо відповідь...");

                var htmlMessage = await FormatCurrencyRatesAsHtmlAsync();
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: htmlMessage,
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation("Відповідь на команду /rates відправлена.");

            }
        }


        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Помилка при обробці оновлення від Telegram.");
            return Task.CompletedTask;
        }

        public async Task<string> FormatCurrencyRatesAsHtmlAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var currencyRateService = scope.ServiceProvider.GetRequiredService<CurrencyService>();

                var ratesTask = currencyRateService.GetCurrencyRatesAsync();
                var monoRatesTask = currencyRateService.GetMonobankCurrencyRatesAsync();
                var NBURatesTask = currencyRateService.GetNbuCurrencyRatesAsync();

                await Task.WhenAll(ratesTask, NBURatesTask);

                var rates = await ratesTask;
                var monoRates = await monoRatesTask;
                var NBURates = await NBURatesTask;

                var sb = new StringBuilder();

                sb.Append("<b>Курси Валют</b>\n");
                sb.Append("<pre>");
                sb.Append("Валюта | Купівля | Продаж\n");
                sb.Append("-------------------------\n");

                sb.Append("<b>Privat Rates</b>\n");

                foreach (var rate in rates)
                {
                    sb.AppendFormat("{0} | {1} | {2}\n", rate.CurrencyCode, rate.BuyRate, rate.SellRate);
                }

                sb.Append("<b>Monobank Rates</b>\n");

                foreach (var rate in monoRates)
                {
                    sb.AppendFormat("{0} | {1} | {2}\n", rate.CurrencyCode, rate.BuyRate, rate.SellRate);
                }

                sb.Append("-------------------------\n");

                sb.Append("<b>NBU Rates</b>\n");

                foreach (var rate in NBURates)
                {
                    sb.AppendFormat("{0} | {1} \n", rate.CurrencyCode, rate.BuyRate);
                }

                sb.Append("</pre>");

                return sb.ToString();
            }

        }

    }
    
}

