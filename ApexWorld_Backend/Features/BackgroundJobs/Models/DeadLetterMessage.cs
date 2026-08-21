using ApexWorld.Core.Common;
using System;

namespace ApexWorld_Backend.Features.BackgroundJobs.Models
{
    public class DeadLetterMessage : BaseEntity
    {
        public string OriginalQueue { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public string Exception { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
        public int RetryCount { get; set; } = 0;
        public string? JobId { get; set; }
    }
}
