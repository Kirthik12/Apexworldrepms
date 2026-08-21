using System;
using System.ComponentModel.DataAnnotations.Schema;
using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Backups.Models
{
    [Table("BackupConfiguration", Schema = "REPMS")]
    public class BackupConfiguration : BaseEntity
    {
        public string Frequency { get; set; } = "Daily"; // Daily, Weekly, Monthly
        public string BackupType { get; set; } = "Full"; // Full, Differential, Log
        public int RetentionDays { get; set; } = 30;
        public string BackupTime { get; set; } = "02:00"; // e.g. "02:00"
        public string StoragePath { get; set; } = @"C:\ApexWorldBackups";
        public bool IsEnabled { get; set; } = true;
        public string CreatedBy { get; set; } = "System";
        public string? UpdatedBy { get; set; }
    }
}
