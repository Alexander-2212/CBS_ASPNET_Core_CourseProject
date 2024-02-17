namespace CBS_ASPNET_Core_CourseProject.Models
{
    public class CurrencyRate
    {
        public int Id { get; set; }
        public string CurrencyCode { get; set; }
        public string BaseCurrencyCode { get; set; }
        public decimal BuyRate { get; set; }
        public decimal SellRate { get; set; }
    }
}
