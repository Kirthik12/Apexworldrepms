using System.Collections.Generic;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Backups.Models;

namespace ApexWorld_Backend.Features.Backups.Services
{
    public interface IBackupService
    {
        Task<BackupHistory> CreateBackupAsync(string backupName, string backupType, string includeData, string createdBy, string? backupDescription = null);
        Task<List<BackupHistory>> GetBackupHistoryAsync();
        Task<BackupHistory?> GetBackupByIdAsync(int id);
        Task DeleteBackupAsync(int id);
        Task<(byte[] FileBytes, string FileName, string ContentType)> DownloadBackupFileAsync(int id);
        Task<object> GetBackupStatusAsync();
        Task<object> GetRestorePreviewAsync(int id);
        Task ExecuteRestoreAsync(int id, string confirmedBy);
        Task<BackupConfiguration> GetBackupSettingsAsync();
        Task<BackupConfiguration> SaveBackupSettingsAsync(BackupConfiguration settings, string updatedBy);
    }
}
