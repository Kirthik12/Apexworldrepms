using System;
using System.Threading;
using System.Threading.Tasks;
using ApexWorld_Backend.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ApexWorld_Backend.Features.Audit.Scheduler
{
    public class AuditLogCleanupScheduler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuditLogCleanupScheduler> _logger;
        private DateTime? _lastRanDate = null;
        private readonly int _retentionDays = 90;

        public AuditLogCleanupScheduler(IServiceProvider serviceProvider, ILogger<AuditLogCleanupScheduler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Audit Log Cleanup Scheduler started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                
                // Run once a day at approximately 3 AM UTC
                if ((_lastRanDate == null || _lastRanDate.Value.Date != now.Date) && now.Hour >= 3)
                {
                    await CleanupLogsAsync(stoppingToken);
                }

                // Sleep for an hour before checking again
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task CleanupLogsAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
                try
                {
                    _logger.LogInformation($"Starting auto-cleanup for Audit Logs older than {_retentionDays} days.");
                    await auditService.CleanupOldLogsAsync(_retentionDays);
                    _logger.LogInformation($"Finished auto-cleanup for Audit Logs.");
                    
                    _lastRanDate = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing Audit Log cleanup job.");
                }
            }
        }
    }
}
