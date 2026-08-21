using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Dashboard.Models
{
    public class DashboardMetric : BaseEntity
    {
        public string Key { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Trend { get; set; } = "stable"; // e.g. up, down, stable
        public string? DisplayName { get; set; }
    }
}
