namespace ApexWorld_Backend.Features.Notifications.DTOs
{
    public class BuyerNotificationDto
    {
        public int Id { get; set; }
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

    public class BuyerNotificationListDto
    {
        public int TotalItems { get; set; }
        public int UnreadCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<BuyerNotificationDto> Items { get; set; } = new();
    }

    public class CreateBuyerNotificationDto
    {
        public int BuyerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string? ActionText { get; set; }
        public string? ActionUrl { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
    }
}
