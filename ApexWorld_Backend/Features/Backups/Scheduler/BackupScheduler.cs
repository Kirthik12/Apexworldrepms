using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Backups.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ApexWorld_Backend.Features.Backups.Scheduler
{
    public class BackupScheduler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackupScheduler> _logger;
        private DateTime? _lastCheckedDate = null;

        public BackupScheduler(IServiceProvider serviceProvider, ILogger<BackupScheduler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Backup Background Scheduler started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                // Run check daily/hourly but run at specific configured time
                if (_lastCheckedDate == null || _lastCheckedDate.Value.Date != now.Date)
                {
                    await CheckAndTriggerBackupAsync(stoppingToken);
                }

                // Sleep for 15 minutes before checking again
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }

        private async Task CheckAndTriggerBackupAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
                try
                {
                    var config = await backupService.GetBackupSettingsAsync();
                    if (config != null && config.IsEnabled)
                    {
                        var today = DateTime.UtcNow.Date;
                        // Parse BackupTime e.g. "02:00"
                        if (TimeSpan.TryParse(config.BackupTime, out var targetTime))
                        {
                            var targetDateTimeUtc = today.Add(targetTime);
                            
                            // If we have reached or passed the target scheduled time for today
                            if (DateTime.UtcNow >= targetDateTimeUtc)
                            {
                                // Check if an automated backup already ran today
                                var history = await backupService.GetBackupHistoryAsync();
                                var alreadyRan = history.Any(h => 
                                    h.CreatedAt.Date == today && 
                                    h.Status == "Success" && 
                                    h.CreatedBy == "System Scheduler");

                                if (!alreadyRan)
                                {
                                    _logger.LogInformation($"Triggering scheduled automated backup: Type={config.BackupType}");
                                    
                                    await backupService.CreateBackupAsync(
                                        $"REPMS_Auto_{config.BackupType}_{today:yyyyMMdd}",
                                        config.BackupType,
                                        "DatabaseOnly", // Default scheduled backup is DatabaseOnly
                                        "System Scheduler",
                                        $"Automated scheduled {config.BackupType} backup"
                                    );

                                    // Cleanup expired backups according to retention days
                                    await CleanupExpiredBackupsAsync(backupService);
                                    
                                    _lastCheckedDate = DateTime.UtcNow;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing scheduled backup job.");
                }
            }
        }

        private async Task CleanupExpiredBackupsAsync(IBackupService backupService)
        {
            try
            {
                var history = await backupService.GetBackupHistoryAsync();
                var expired = history.Where(h => h.Status == "Success" && h.RetentionUntil < DateTime.UtcNow).ToList();
                
                foreach (var backup in expired)
                {
                    _logger.LogInformation($"Auto-deleting expired backup: ID={backup.Id}, Name={backup.BackupName}");
                    try
                    {
                        await backupService.DeleteBackupAsync(backup.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to auto-delete expired backup ID={backup.Id}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during retention policy cleanup.");
            }
        }
    }
}
