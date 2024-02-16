using System.Text.Json;

namespace CBS_ASPNET_Core_CourseProject.Services
{
    public class CurrencyService
    {
        public readonly HttpClient _httpClient;

        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
    }
}
