using ApexWorld.Core.Common;
using System;

namespace ApexWorld_Backend.Features.Notifications.Models
{
    public class AdminNotification : BaseEntity
    {
        public int AdminId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string? ActionText { get; set; }
        public string? ActionUrl { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
