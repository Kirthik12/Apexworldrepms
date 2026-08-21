using System;
using System.Collections.Generic;

namespace ApexWorld_Backend.Features.Notifications.DTOs
{
    public class AdminNotificationDto
    {
        public int Id { get; set; }
        public int AdminId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? ActionText { get; set; }
        public string? ActionUrl { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class AdminNotificationListDto
    {
        public int TotalItems { get; set; }
        public int UnreadCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<AdminNotificationDto> Items { get; set; } = new();
    }

    public class BroadcastNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Category { get; set; } = "Announcement";
        public string TargetAudience { get; set; } = "AllUsers"; // AllUsers, Buyers, Admins, SpecificRole, SpecificUser
        public string? TargetRole { get; set; } // If TargetAudience == SpecificRole
        public int? TargetUserId { get; set; } // If TargetAudience == SpecificUser
    }
}
