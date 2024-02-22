using CBS_ASPNET_Core_CourseProject.Entities;

namespace CBS_ASPNET_Core_CourseProject.Models
{
    public class CurrencyRatesViewModel
    {
        public IEnumerable<CurrencyRate> PrivatBankRates { get; set; }
        public IEnumerable<CurrencyRate> MonobankRates { get; set; }
        public IEnumerable<CurrencyRate> NbuRates { get; set; }
    }
}
