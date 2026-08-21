using System;
using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Reports.Models{
    public class Report : BaseEntity
    {
        public string ReportName { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string PropertyScope { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "Scheduled"; // Scheduled, Processing, Completed, Failed
        public string GeneratedBy { get; set; } = "Admin";
        public string Format { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public string DataPayload { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
