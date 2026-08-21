using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Backups.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ApexWorld_Backend.Features.Backups.Services
{
    public class BackupService : IBackupService
    {
        private readonly IRepository<BackupHistory> _historyRepository;
        private readonly IRepository<BackupConfiguration> _configRepository;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public BackupService(
            IRepository<BackupHistory> historyRepository,
            IRepository<BackupConfiguration> configRepository,
            IAuditService auditService,
            IConfiguration configuration)
        {
            _historyRepository = historyRepository;
            _configRepository = configRepository;
            _auditService = auditService;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=KIRTHIK;Database=ApexWorldREPMS;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        }

        private string GetMasterConnectionString()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString)
            {
                InitialCatalog = "master"
            };
            return builder.ConnectionString;
        }

        public async Task<BackupConfiguration> GetBackupSettingsAsync()
        {
            var configs = await _configRepository.GetAllAsync();
            var config = configs.FirstOrDefault(c => !c.IsDeleted);
            if (config == null)
            {
                config = new BackupConfiguration
                {
                    Frequency = "Daily",
                    BackupType = "Full",
                    RetentionDays = 30,
                    BackupTime = "02:00",
                    StoragePath = @"C:\ApexWorldBackups",
                    IsEnabled = true,
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                };
                await _configRepository.AddAsync(config);
            }
            return config;
        }

        public async Task<BackupConfiguration> SaveBackupSettingsAsync(BackupConfiguration settings, string updatedBy)
        {
            var config = await GetBackupSettingsAsync();
            config.Frequency = settings.Frequency;
            config.BackupType = settings.BackupType;
            config.RetentionDays = settings.RetentionDays;
            config.BackupTime = settings.BackupTime;
            config.StoragePath = settings.StoragePath;
            config.IsEnabled = settings.IsEnabled;
            config.UpdatedBy = updatedBy;
            config.UpdatedAt = DateTime.UtcNow;

            await _configRepository.UpdateAsync(config);
            await _auditService.LogAsync("BackupSettingsUpdated", "Backup", config.Id.ToString(), $"Frequency: {config.Frequency}, Type: {config.BackupType}, StoragePath: {config.StoragePath}", updatedBy);
            return config;
        }

        public async Task<List<BackupHistory>> GetBackupHistoryAsync()
        {
            var list = await _historyRepository.GetAllAsync();
            return list.Where(b => !b.IsDeleted).OrderByDescending(b => b.CreatedAt).ToList();
        }

        public async Task<BackupHistory?> GetBackupByIdAsync(int id)
        {
            var backup = await _historyRepository.GetByIdAsync(id);
            if (backup == null || backup.IsDeleted) return null;
            return backup;
        }

        public async Task DeleteBackupAsync(int id)
        {
            var backup = await GetBackupByIdAsync(id);
            if (backup == null) throw new KeyNotFoundException("Backup not found");

            // Validate that we aren't deleting a Full backup that is required by Differential or Log backups
            if (backup.BackupType == "Full")
            {
                var histories = await GetBackupHistoryAsync();
                var dependents = histories.Any(h => h.ParentBackupId == backup.Id && h.Status == "Success");
                if (dependents)
                {
                    throw new InvalidOperationException("Cannot delete this Full backup because it is required by differential or log backups in its chain.");
                }
            }

            backup.IsDeleted = true;
            backup.UpdatedAt = DateTime.UtcNow;
            await _historyRepository.UpdateAsync(backup);

            // Attempt to delete physical file if exists
            try
            {
                if (File.Exists(backup.FilePath))
                {
                    File.Delete(backup.FilePath);
                }
            }
            catch (Exception)
            {
                // Soft delete metadata even if file deletion fails
            }

            await _auditService.LogAsync("BackupDeleted", "Backup", id.ToString(), $"Name: {backup.BackupName}", backup.CreatedBy);
        }

        public async Task<BackupHistory> CreateBackupAsync(string backupName, string backupType, string includeData, string createdBy, string? backupDescription = null)
        {
            var settings = await GetBackupSettingsAsync();
            var storagePath = settings.StoragePath;
            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var cleanBackupName = backupName.Replace(" ", "_");
            var finalFilePath = "";
            var size = 0L;
            var checksum = "";
            var status = "Success";
            string? errorMessage = null;

            int? parentBackupId = null;
            if (backupType != "Full" && includeData != "FilesOnly")
            {
                // Find parent Full backup
                var history = await GetBackupHistoryAsync();
                var parent = history.FirstOrDefault(b => b.BackupType == "Full" && b.IncludeData != "FilesOnly" && b.Status == "Success");
                if (parent == null)
                {
                    throw new InvalidOperationException($"Cannot create {backupType} backup because no successful Full Backup was found.");
                }
                parentBackupId = parent.Id;
            }

            try
            {
                if (includeData == "DatabaseOnly")
                {
                    var ext = backupType == "Log" ? "trn" : "bak";
                    finalFilePath = Path.Combine(storagePath, $"REPMS_{backupType}_{timestamp}.{ext}");
                    await BackupDatabaseAsync(finalFilePath, backupType);
                }
                else if (includeData == "FilesOnly")
                {
                    finalFilePath = Path.Combine(storagePath, $"REPMS_Files_{timestamp}.zip");
                    await BackupFilesAsync(finalFilePath);
                }
                else // AllData
                {
                    // Create Db backup temporarily, zip it together with application files, and delete the temp bak
                    var dbBakPath = Path.Combine(storagePath, $"REPMS_DbTemp_{timestamp}.bak");
                    await BackupDatabaseAsync(dbBakPath, backupType);

                    finalFilePath = Path.Combine(storagePath, $"REPMS_AllData_{timestamp}.zip");
                    await BackupAllDataAsync(dbBakPath, finalFilePath);
                }

                if (File.Exists(finalFilePath))
                {
                    var fileInfo = new FileInfo(finalFilePath);
                    size = fileInfo.Length;
                    checksum = CalculateSHA256(finalFilePath);
                }
            }
            catch (Exception ex)
            {
                status = "Failed";
                errorMessage = ex.Message;
                // Log failed backup history
            }

            var historyRecord = new BackupHistory
            {
                BackupName = backupName,
                BackupType = backupType,
                IncludeData = includeData,
                FilePath = finalFilePath,
                FileSize = size,
                CreatedBy = createdBy,
                Status = status,
                ErrorMessage = errorMessage,
                ParentBackupId = parentBackupId,
                RetentionUntil = DateTime.UtcNow.AddDays(settings.RetentionDays),
                Checksum = checksum,
                CreatedAt = DateTime.UtcNow
            };

            await _historyRepository.AddAsync(historyRecord);
            await _auditService.LogAsync("BackupCreated", "Backup", historyRecord.Id.ToString(), $"Name: {backupName}, Type: {backupType}, IncludeData: {includeData}, Status: {status}", createdBy);

            if (status == "Failed")
            {
                throw new Exception($"Backup failed: {errorMessage}");
            }

            return historyRecord;
        }

        private async Task BackupDatabaseAsync(string filePath, string backupType)
        {
            var dbName = "ApexWorldREPMS";
            var query = "";

            if (backupType == "Full")
            {
                query = $"BACKUP DATABASE [{dbName}] TO DISK = @path WITH COMPRESSION, CHECKSUM, INIT;";
            }
            else if (backupType == "Differential")
            {
                query = $"BACKUP DATABASE [{dbName}] TO DISK = @path WITH DIFFERENTIAL, COMPRESSION, CHECKSUM, INIT;";
            }
            else if (backupType == "Log" || backupType == "Incremental")
            {
                query = $"BACKUP LOG [{dbName}] TO DISK = @path WITH COMPRESSION, CHECKSUM, INIT;";
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@path", filePath);
                    cmd.CommandTimeout = 300; // 5 minutes
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private Task BackupFilesAsync(string zipPath)
        {
            var mediaPath = _configuration["Storage:PropertyImagesPath"] 
                ?? _configuration["Storage__PropertyImagesPath"] 
                ?? @"C:\ApexWorld\Storage\Images";

            if (!Directory.Exists(mediaPath))
            {
                Directory.CreateDirectory(mediaPath);
            }

            // Create ZIP file
            if (File.Exists(zipPath)) File.Delete(zipPath);
            ZipFile.CreateFromDirectory(mediaPath, zipPath);
            return Task.CompletedTask;
        }

        private Task BackupAllDataAsync(string dbBakPath, string zipPath)
        {
            var mediaPath = _configuration["Storage:PropertyImagesPath"] 
                ?? _configuration["Storage__PropertyImagesPath"] 
                ?? @"C:\ApexWorld\Storage\Images";

            if (!Directory.Exists(mediaPath))
            {
                Directory.CreateDirectory(mediaPath);
            }

            // Create a temp staging directory
            var tempStaging = Path.Combine(Path.GetTempPath(), "REPMS_Backup_" + Guid.NewGuid());
            Directory.CreateDirectory(tempStaging);

            try
            {
                // Copy media files to temp staging
                var mediaDest = Path.Combine(tempStaging, "Files");
                Directory.CreateDirectory(mediaDest);
                foreach (var file in Directory.GetFiles(mediaPath, "*.*", SearchOption.AllDirectories))
                {
                    var rel = Path.GetRelativePath(mediaPath, file);
                    var dest = Path.Combine(mediaDest, rel);
                    var destDir = Path.GetDirectoryName(dest);
                    if (destDir != null && !Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                    File.Copy(file, dest);
                }

                // Copy Db backup to temp staging
                if (File.Exists(dbBakPath))
                {
                    File.Copy(dbBakPath, Path.Combine(tempStaging, "database.bak"));
                    File.Delete(dbBakPath); // clean up the loose bak
                }

                if (File.Exists(zipPath)) File.Delete(zipPath);
                ZipFile.CreateFromDirectory(tempStaging, zipPath);
            }
            finally
            {
                if (Directory.Exists(tempStaging))
                {
                    Directory.Delete(tempStaging, true);
                }
            }
            return Task.CompletedTask;
        }

        public async Task<(byte[] FileBytes, string FileName, string ContentType)> DownloadBackupFileAsync(int id)
        {
            var backup = await GetBackupByIdAsync(id);
            if (backup == null || !File.Exists(backup.FilePath))
            {
                throw new FileNotFoundException("Backup archive file not found.");
            }

            var bytes = await File.ReadAllBytesAsync(backup.FilePath);
            var name = Path.GetFileName(backup.FilePath);
            return (bytes, name, "application/octet-stream");
        }

        public async Task<object> GetBackupStatusAsync()
        {
            var history = await GetBackupHistoryAsync();
            var successList = history.Where(h => h.Status == "Success").ToList();
            var last = successList.FirstOrDefault();

            var settings = await GetBackupSettingsAsync();
            var totalStorage = 50.0; // 50GB configuration mock or actual drive space
            var usedStorage = 0.0;

            try
            {
                var dir = new DirectoryInfo(settings.StoragePath);
                if (dir.Exists)
                {
                    var files = dir.GetFiles("*.*", SearchOption.AllDirectories);
                    usedStorage = files.Sum(f => f.Length) / 1024.0 / 1024.0 / 1024.0; // GB
                }
            }
            catch (Exception)
            {
                usedStorage = successList.Sum(s => s.FileSize) / 1024.0 / 1024.0 / 1024.0; // GB
            }

            var nextBackup = DateTime.UtcNow;
            if (settings.IsEnabled)
            {
                var todayStr = DateTime.UtcNow.ToString("yyyy-MM-dd");
                if (DateTime.TryParse($"{todayStr} {settings.BackupTime}", out var schedTime))
                {
                    if (schedTime < DateTime.UtcNow)
                    {
                        nextBackup = schedTime.AddDays(settings.Frequency == "Weekly" ? 7 : settings.Frequency == "Monthly" ? 30 : 1);
                    }
                    else
                    {
                        nextBackup = schedTime;
                    }
                }
            }

            return new
            {
                lastBackupDate = last?.CreatedAt,
                nextScheduledBackup = settings.IsEnabled ? nextBackup : (DateTime?)null,
                status = last?.Status ?? "Success",
                usedStorageGB = Math.Round(usedStorage, 2),
                totalStorageGB = totalStorage,
                percentageUsed = Math.Min(100.0, Math.Round((usedStorage / totalStorage) * 100.0, 1))
            };
        }

        public async Task<object> GetRestorePreviewAsync(int id)
        {
            var backup = await GetBackupByIdAsync(id);
            if (backup == null) throw new KeyNotFoundException("Backup not found");

            var chain = new List<BackupHistory>();
            var isValid = true;
            var validationMessage = "Valid chain.";

            if (backup.IncludeData != "FilesOnly")
            {
                // Walk the chain for restoration
                if (backup.BackupType == "Full")
                {
                    chain.Add(backup);
                }
                else
                {
                    // For log or diff backups, we need to gather dependencies
                    var all = await GetBackupHistoryAsync();
                    var parentFull = all.FirstOrDefault(b => b.Id == backup.ParentBackupId && b.Status == "Success");
                    if (parentFull == null)
                    {
                        isValid = false;
                        validationMessage = "Parent Full Backup is missing or failed.";
                    }
                    else
                    {
                        chain.Add(parentFull);
                        // If it is Log backup, we might also need the intermediate Differential backup if any, or previous log backups.
                        // For simplicity in SQL Server: we restore Full -> Diff (if any) -> all Logs sequentially.
                        if (backup.BackupType == "Log")
                        {
                            // Find any intermediate differential backups
                            var diff = all.Where(b => b.ParentBackupId == parentFull.Id && b.BackupType == "Differential" && b.CreatedAt < backup.CreatedAt && b.Status == "Success")
                                          .OrderByDescending(b => b.CreatedAt)
                                          .FirstOrDefault();
                            
                            DateTime startLogsFrom = parentFull.CreatedAt;
                            if (diff != null)
                            {
                                chain.Add(diff);
                                startLogsFrom = diff.CreatedAt;
                            }

                            var logs = all.Where(b => b.ParentBackupId == parentFull.Id && b.BackupType == "Log" && b.CreatedAt > startLogsFrom && b.CreatedAt <= backup.CreatedAt && b.Status == "Success")
                                          .OrderBy(b => b.CreatedAt)
                                          .ToList();
                            
                            foreach (var log in logs)
                            {
                                if (!chain.Any(x => x.Id == log.Id))
                                {
                                    chain.Add(log);
                                }
                            }

                            if (!chain.Any(x => x.Id == backup.Id))
                            {
                                chain.Add(backup);
                            }
                        }
                        else if (backup.BackupType == "Differential")
                        {
                            chain.Add(backup);
                        }
                    }
                }
            }
            else
            {
                chain.Add(backup);
            }

            // Verify file availability for all files in the chain
            foreach (var item in chain)
            {
                if (!File.Exists(item.FilePath))
                {
                    isValid = false;
                    validationMessage = $"File not found on disk: {Path.GetFileName(item.FilePath)}";
                    break;
                }
            }

            var estSeconds = chain.Count * 15; // Rough estimate of 15 seconds per restore step

            return new
            {
                isValid,
                validationMessage,
                estimatedRestoreTimeSeconds = estSeconds,
                requiredBackupChain = chain.Select(c => new
                {
                    c.Id,
                    c.BackupName,
                    c.BackupType,
                    c.IncludeData,
                    c.CreatedAt,
                    c.FileSize,
                    FileExists = File.Exists(c.FilePath)
                }).ToList()
            };
        }

        public async Task ExecuteRestoreAsync(int id, string confirmedBy)
        {
            var previewObj = await GetRestorePreviewAsync(id);
            dynamic preview = previewObj;
            if (!preview.isValid)
            {
                throw new InvalidOperationException($"Cannot restore backup: {preview.validationMessage}");
            }

            var backup = await GetBackupByIdAsync(id);
            if (backup == null) throw new KeyNotFoundException("Backup not found");

            // 1. Create Pre-Restore Safety Backup
            try
            {
                await CreateBackupAsync($"REPMS_PreRestore_{DateTime.UtcNow:yyyyMMdd_HHmmss}", "Full", "DatabaseOnly", confirmedBy, "Automated safety backup before restoration");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Pre-restore safety backup failed. Restore cancelled: {ex.Message}");
            }

            // 2. Perform Sequential Restores
            if (backup.IncludeData == "DatabaseOnly" || backup.IncludeData == "AllData")
            {
                var listChain = new List<BackupHistory>();
                foreach (var item in preview.requiredBackupChain)
                {
                    int itemId = item.Id;
                    var dbRecord = await GetBackupByIdAsync(itemId);
                    if (dbRecord != null) listChain.Add(dbRecord);
                }

                await RestoreDatabaseChainAsync(listChain);
            }

            // 3. Restore Application Files
            if (backup.IncludeData == "FilesOnly" || backup.IncludeData == "AllData")
            {
                await RestoreApplicationFilesAsync(backup.FilePath, backup.IncludeData == "AllData");
            }

            await _auditService.LogAsync("RestoreCompleted", "Backup", id.ToString(), $"Restored successfully by {confirmedBy}", confirmedBy);
        }

        private async Task RestoreDatabaseChainAsync(List<BackupHistory> chain)
        {
            var masterConnString = GetMasterConnectionString();
            var dbName = "ApexWorldREPMS";

            using (var conn = new SqlConnection(masterConnString))
            {
                await conn.OpenAsync();

                // Bring DB offline (kill all active connections)
                using (var offlineCmd = new SqlCommand($"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", conn))
                {
                    await offlineCmd.ExecuteNonQueryAsync();
                }

                try
                {
                    for (int i = 0; i < chain.Count; i++)
                    {
                        var item = chain[i];
                        var isLast = (i == chain.Count - 1);
                        var recoveryOption = isLast ? "RECOVERY" : "NORECOVERY";
                        var query = "";

                        if (item.BackupType == "Full")
                        {
                            query = $"RESTORE DATABASE [{dbName}] FROM DISK = @path WITH {recoveryOption}, REPLACE;";
                        }
                        else if (item.BackupType == "Differential")
                        {
                            query = $"RESTORE DATABASE [{dbName}] FROM DISK = @path WITH {recoveryOption};";
                        }
                        else if (item.BackupType == "Log" || item.BackupType == "Incremental")
                        {
                            query = $"RESTORE LOG [{dbName}] FROM DISK = @path WITH {recoveryOption};";
                        }

                        using (var restoreCmd = new SqlCommand(query, conn))
                        {
                            restoreCmd.Parameters.AddWithValue("@path", item.FilePath);
                            restoreCmd.CommandTimeout = 300;
                            await restoreCmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                finally
                {
                    // Bring DB online
                    using (var onlineCmd = new SqlCommand($"ALTER DATABASE [{dbName}] SET MULTI_USER;", conn))
                    {
                        await onlineCmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        private Task RestoreApplicationFilesAsync(string archivePath, bool isNestedInAllData)
        {
            var mediaPath = _configuration["Storage:PropertyImagesPath"] 
                ?? _configuration["Storage__PropertyImagesPath"] 
                ?? @"C:\ApexWorld\Storage\Images";

            if (!Directory.Exists(mediaPath))
            {
                Directory.CreateDirectory(mediaPath);
            }

            if (!File.Exists(archivePath)) return Task.CompletedTask;

            if (!isNestedInAllData && archivePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                // Clean media directory
                Directory.Delete(mediaPath, true);
                Directory.CreateDirectory(mediaPath);
                ZipFile.ExtractToDirectory(archivePath, mediaPath);
            }
            else if (isNestedInAllData && archivePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                // Staging area extract
                var tempExtract = Path.Combine(Path.GetTempPath(), "REPMS_Extract_" + Guid.NewGuid());
                Directory.CreateDirectory(tempExtract);
                try
                {
                    ZipFile.ExtractToDirectory(archivePath, tempExtract);
                    var filesSource = Path.Combine(tempExtract, "Files");
                    if (Directory.Exists(filesSource))
                    {
                        Directory.Delete(mediaPath, true);
                        Directory.CreateDirectory(mediaPath);
                        foreach (var file in Directory.GetFiles(filesSource, "*.*", SearchOption.AllDirectories))
                        {
                            var rel = Path.GetRelativePath(filesSource, file);
                            var dest = Path.Combine(mediaPath, rel);
                            var destDir = Path.GetDirectoryName(dest);
                            if (destDir != null && !Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                            File.Copy(file, dest);
                        }
                    }
                }
                finally
                {
                    if (Directory.Exists(tempExtract)) Directory.Delete(tempExtract, true);
                }
            }

            return Task.CompletedTask;
        }

        private static string CalculateSHA256(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hashBytes = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
