namespace CBS_ASPNET_Core_CourseProject.Models
{
    public class MonobankCurrencyRate
    {
        public int currencyCodeA { get; set; }
        public int currencyCodeB { get; set; }
        public long date { get; set; }
        public decimal rateSell { get; set; }
        public decimal rateBuy { get; set; }
        public decimal rateCross { get; set; }
    }
}
