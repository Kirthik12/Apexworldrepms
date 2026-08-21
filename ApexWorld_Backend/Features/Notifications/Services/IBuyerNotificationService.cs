using ApexWorld_Backend.Features.Notifications.DTOs;

namespace ApexWorld_Backend.Features.Notifications.Services
{
    public interface IBuyerNotificationService
    {
        Task<BuyerNotificationListDto> GetBuyerNotificationsAsync(string buyerId, string? category, bool unreadOnly, int pageNumber, int pageSize);
        Task<BuyerNotificationDto> GetBuyerNotificationByIdAsync(string buyerId, int notificationId);
        Task<BuyerNotificationDto> CreateBuyerNotificationAsync(CreateBuyerNotificationDto request);
        Task MarkAsReadAsync(string buyerId, int notificationId);
        Task MarkAllAsReadAsync(string buyerId);
    }
}
