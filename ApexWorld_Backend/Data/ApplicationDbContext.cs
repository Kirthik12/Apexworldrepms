using ApexWorld.Core.Common;
using Microsoft.EntityFrameworkCore;
using ApexWorld_Backend.Features.Users.Models;
using ApexWorld_Backend.Features.Property.Models;
using ApexWorld_Backend.Features.Booking.Models;
using ApexWorld_Backend.Features.Audit.Models;
using ApexWorld_Backend.Features.Enquiry.Models;
using ApexWorld_Backend.Features.Loan.Models;
using ApexWorld_Backend.Features.Payment.Models;
using ApexWorld_Backend.Features.Review.Models;
using ApexWorld_Backend.Features.Wishlist.Models;
using ApexWorld_Backend.Features.Reports.Models;
using ApexWorld_Backend.Features.Roles.Models;
using ApexWorld_Backend.Features.Notifications.Models;

using ApexWorld_Backend.Features.Webhooks.Models;
using ApexWorld_Backend.Features.BackgroundJobs.Models;
using ApexWorld_Backend.Features.Dashboard.Models;
using ApexWorld_Backend.Features.ContentManagement.Models;
using ApexWorld_Backend.Features.Backups.Models;

namespace ApexWorld_Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<RevokedToken> RevokedTokens { get; set; }

        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }
        public DbSet<WebhookDeliveryLog> WebhookDeliveryLogs { get; set; }

        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyCategory> PropertyCategories { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }

        public DbSet<Booking> Bookings { get; set; }
        
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Enquiry> Enquiries { get; set; }
        
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<EMIPlan> EMIPlans { get; set; }
        public DbSet<Policy> Policies { get; set; }
        
        public DbSet<PaymentRecord> Payments { get; set; }
        public DbSet<PaymentHistory> PaymentHistory { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        
        public DbSet<Review> Reviews { get; set; }
        
        public DbSet<Wishlist> Wishlists { get; set; }
        
        public DbSet<Report> Reports { get; set; }

        public DbSet<BuyerNotification> BuyerNotifications { get; set; }
        public DbSet<AdminNotification> AdminNotifications { get; set; }
        
        public DbSet<DeadLetterMessage> DeadLetterMessages { get; set; }

        public DbSet<DashboardMetric> DashboardMetrics { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<BackupHistory> BackupHistories { get; set; }
        public DbSet<BackupConfiguration> BackupConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            if (Database.IsRelational())
            {
                // Apply standard decimal precision across all decimal properties
                foreach (var property in modelBuilder.Model.GetEntityTypes()
                    .SelectMany(t => t.GetProperties())
                    .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
                {
                    property.SetColumnType("decimal(18,2)");
                }

                modelBuilder.HasDefaultSchema("REPMS");
            }
            
            if (Database.IsRelational())
            {
                // TPT Configuration
                modelBuilder.Entity<User>().UseTptMappingStrategy();
                modelBuilder.Entity<PaymentRecord>().ToTable("Payments");
            }
            
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<Receipt>().HasOne(r => r.PaymentRecord).WithOne(p => p.Receipt).HasForeignKey<Receipt>(r => r.PaymentId);

            // Global Query Filters for Soft Delete
            modelBuilder.Entity<Property>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Booking>().HasQueryFilter(b => !b.IsDeleted);

            // DB Optimization: Add Indexes for read-heavy operations
            modelBuilder.Entity<Property>().HasIndex(p => p.Status);
            modelBuilder.Entity<Property>().HasIndex(p => p.IsAvailable);
            modelBuilder.Entity<Booking>().HasIndex(b => b.BuyerId);
            modelBuilder.Entity<Booking>().HasIndex(b => b.Status);
            modelBuilder.Entity<BuyerNotification>().HasIndex(n => n.BuyerId);
            modelBuilder.Entity<BuyerNotification>().HasIndex(n => n.IsRead);
            modelBuilder.Entity<BuyerNotification>().HasIndex(n => n.Category);

            modelBuilder.Entity<AdminNotification>().HasIndex(n => n.AdminId);
            modelBuilder.Entity<AdminNotification>().HasIndex(n => n.IsRead);
            modelBuilder.Entity<AdminNotification>().HasIndex(n => n.Category);

            // Configure RowVersion for Optimistic Concurrency
            if (Database.IsRelational())
            {
                modelBuilder.Entity<Property>()
                    .Property(p => p.RowVersion)
                    .IsRowVersion();
                    
                modelBuilder.Entity<Booking>()
                    .Property(b => b.RowVersion)
                    .IsRowVersion();
            }

            // Configure Review Foreign Key and Unique Index Constraint
            modelBuilder.Entity<Review>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.BuyerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.BuyerId, r.PropertyId })
                .HasFilter("[ReviewType] = 'Property'")
                .IsUnique();

            // Seed Dashboard Metrics Data
            modelBuilder.Entity<DashboardMetric>().HasData(
                new DashboardMetric { Id = 1, Key = "ActiveListings", Value = 13, Category = "Listings", Trend = "up", DisplayName = "Active Listings", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new DashboardMetric { Id = 2, Key = "TotalCompletedRevenue", Value = 5.79m, Category = "Revenue", Trend = "up", DisplayName = "Total Completed Revenue (Cr)", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new DashboardMetric { Id = 3, Key = "PendingLoanApplications", Value = 5, Category = "Loans", Trend = "stable", DisplayName = "Pending Loan Applications", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new DashboardMetric { Id = 4, Key = "UnresolvedEnquiries", Value = 4, Category = "Enquiries", Trend = "down", DisplayName = "Unresolved Enquiries", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}



