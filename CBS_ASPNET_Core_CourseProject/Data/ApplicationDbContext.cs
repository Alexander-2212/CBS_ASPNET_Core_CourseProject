using CBS_ASPNET_Core_CourseProject.Entities;
using Microsoft.EntityFrameworkCore;

namespace CBS_ASPNET_Core_CourseProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<CurrencyRate> CurrencyRates { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }


    }
}
