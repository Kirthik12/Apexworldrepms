using System.Threading.Tasks;
using ApexWorld_Backend.Features.Notifications.DTOs;

namespace ApexWorld_Backend.Features.Notifications.Services
{
    public interface IAdminNotificationService
    {
        Task<AdminNotificationListDto> GetAdminNotificationsAsync(int adminId, string? category, bool unreadOnly, int pageNumber, int pageSize);
        Task<AdminNotificationDto> GetAdminNotificationByIdAsync(int adminId, int notificationId);
        Task MarkAsReadAsync(int adminId, int notificationId);
        Task MarkAllAsReadAsync(int adminId);
        
        Task BroadcastNotificationAsync(BroadcastNotificationDto dto, int senderAdminId);
    }
}
