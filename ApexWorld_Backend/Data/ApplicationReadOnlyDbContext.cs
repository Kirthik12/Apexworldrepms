using Microsoft.EntityFrameworkCore;

namespace ApexWorld_Backend.Data
{
    public class ApplicationReadOnlyDbContext : ApplicationDbContext
    {
        public ApplicationReadOnlyDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }
    }
}
