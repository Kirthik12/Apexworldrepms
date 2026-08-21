using System;
using System.ComponentModel.DataAnnotations.Schema;
using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Backups.Models
{
    [Table("BackupHistory", Schema = "REPMS")]
    public class BackupHistory : BaseEntity
    {
        public string BackupName { get; set; } = string.Empty;
        public string BackupType { get; set; } = string.Empty; // Full, Differential, Log
        public string IncludeData { get; set; } = string.Empty; // DatabaseOnly, FilesOnly, AllData
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Success, Failed
        public string? ErrorMessage { get; set; }
        public int? ParentBackupId { get; set; }
        public DateTime RetentionUntil { get; set; }
        public string? Checksum { get; set; }
    }
}
