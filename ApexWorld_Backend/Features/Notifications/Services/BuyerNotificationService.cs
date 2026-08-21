using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Common.Exceptions;
using ApexWorld_Backend.Features.Notifications.DTOs;
using ApexWorld_Backend.Features.Notifications.Models;

namespace ApexWorld_Backend.Features.Notifications.Services
{
    public class BuyerNotificationService : IBuyerNotificationService
    {
        private readonly IRepository<BuyerNotification> _notificationRepo;

        public BuyerNotificationService(IRepository<BuyerNotification> notificationRepo)
        {
            _notificationRepo = notificationRepo;
        }

        public async Task<BuyerNotificationListDto> GetBuyerNotificationsAsync(string buyerId, string? category, bool unreadOnly, int pageNumber, int pageSize)
        {
            var bId = ParseBuyerId(buyerId);
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var notifications = await _notificationRepo.GetAsync(n => n.BuyerId == bId && !n.IsDeleted);
            var filtered = notifications.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(n => n.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            if (unreadOnly)
            {
                filtered = filtered.Where(n => !n.IsRead);
            }

            var ordered = filtered.OrderByDescending(n => n.CreatedAt).ToList();

            return new BuyerNotificationListDto
            {
                TotalItems = ordered.Count,
                UnreadCount = notifications.Count(n => !n.IsRead),
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = ordered
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ToDto)
                    .ToList()
            };
        }

        public async Task<BuyerNotificationDto> GetBuyerNotificationByIdAsync(string buyerId, int notificationId)
        {
            var bId = ParseBuyerId(buyerId);
            var notifications = await _notificationRepo.GetAsync(n => n.Id == notificationId && n.BuyerId == bId && !n.IsDeleted);
            var notification = notifications.FirstOrDefault();
            if (notification == null) throw new NotFoundException("Notification not found.");

            return ToDto(notification);
        }

        public async Task<BuyerNotificationDto> CreateBuyerNotificationAsync(CreateBuyerNotificationDto request)
        {
            if (request.BuyerId <= 0) throw new ArgumentException("Invalid buyer ID.");
            if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Notification title is required.");
            if (string.IsNullOrWhiteSpace(request.Message)) throw new ArgumentException("Notification message is required.");

            var notification = new BuyerNotification
            {
                BuyerId = request.BuyerId,
                Title = request.Title.Trim(),
                Message = request.Message.Trim(),
                Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim(),
                ActionText = request.ActionText,
                ActionUrl = request.ActionUrl,
                RelatedEntityType = request.RelatedEntityType,
                RelatedEntityId = request.RelatedEntityId
            };

            await _notificationRepo.AddAsync(notification);
            return ToDto(notification);
        }

        public async Task MarkAsReadAsync(string buyerId, int notificationId)
        {
            var bId = ParseBuyerId(buyerId);
            var notifications = await _notificationRepo.GetAsync(n => n.Id == notificationId && n.BuyerId == bId && !n.IsDeleted);
            var notification = notifications.FirstOrDefault();
            if (notification == null) throw new NotFoundException("Notification not found.");

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _notificationRepo.UpdateAsync(notification);
            }
        }

        public async Task MarkAllAsReadAsync(string buyerId)
        {
            var bId = ParseBuyerId(buyerId);
            var notifications = await _notificationRepo.GetAsync(n => n.BuyerId == bId && !n.IsDeleted && !n.IsRead);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _notificationRepo.UpdateAsync(notification);
            }
        }

        private static int ParseBuyerId(string buyerId)
        {
            if (!int.TryParse(buyerId, out var bId)) throw new ArgumentException("Invalid buyer ID.");
            return bId;
        }

        private static BuyerNotificationDto ToDto(BuyerNotification notification)
        {
            return new BuyerNotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Category = notification.Category,
                ActionText = notification.ActionText,
                ActionUrl = notification.ActionUrl,
                RelatedEntityType = notification.RelatedEntityType,
                RelatedEntityId = notification.RelatedEntityId,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt
            };
        }
    }
}
